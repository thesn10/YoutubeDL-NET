using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Text.RegularExpressions;
using YoutubeDL.Models;

namespace YoutubeDL
{
    public enum FormatType
    {
        Pickfirst,
        Merge,
        Single,
        Group,
    }

    public class FormatSelector
    {
        public FormatSelector()
        {

        }

        public FormatSelector(FormatType type, object selector)
        {
            Type = type;
            Selector = selector;
        }

        public FormatSelector(FormatType type, object selector, List<string> filter)
        {
            Type = type;
            Selector = selector;
            Filter = filter;
        }

        public FormatType Type { get; set; }
        public object Selector { get; set; }
        public List<string> Filter { get; set; } = new List<string>();
    }

    public static class FormatParser
    {
        public static IList<IFormat> SelectFormats(IList<IFormat> formats, string format_spec, string mergeOutputFormat = null)
        {
            var selector = BuildFormatSelector(format_spec, mergeOutputFormat);
            return selector(formats);
        }

        public static Func<IList<IFormat>, IList<IFormat>> BuildFormatSelector(string format_spec, string mergeOutputFormat = null)
        {
            var tokens = Regex.Split(format_spec, @"(?=[/+,()\[\]])|(?<=[/+,()\[\]])");
            var index = -1;
            var fs = ParseFormatSelection(tokens, ref index);
            return BuildSelectorFunction(fs, mergeOutputFormat);
        }

        public static string ParseFormatFilter(string[] tokens, ref int index)
        {
            List<string> filter_parts = new List<string>();
            for (index++; index < tokens.Length; index++)
            {
                if (tokens[index] == @"]") 
                {
                    return string.Join("", filter_parts);
                }
                else
                {
                    filter_parts.Add(tokens[index]);
                }
            }
            throw new Exception("No closing square Bracket found");
        }

        public static Func<IFormat, bool> BuildFormatFilter(string filter_spec)
        {
            var operators = new Dictionary<string, Func<int, int, bool>>
            {
                { "<", (x, y) => x < y },
                { ">", (x, y) => x > y },
                { "=", (x, y) => x == y },
                { "<=", (x, y) => x <= y },
                { ">=", (x, y) => x >= y },
                { "!=", (x, y) => x != y },
            };

            string operator_rex =
                @"(?x)\s*(?P<key>width|height|tbr|abr|vbr|asr|filesize|filesize_approx|fps)\s*(?P<op>" + string.Join("|", operators.Keys.Select(x => Regex.Escape(x))) +
                @")(?P<none_inclusive>\s*\?)?\s*(?P<value>[0-9.]+(?:[kKmMgGtTpPeEzZyY]i?[Bb]?)?)$";

            object comp_val = null;
            Func<object, object, bool> op = null;
            Match m = Regex.Match(filter_spec, operator_rex);
            if (m.Success)
            {
                if (int.TryParse(m.Groups["value"].Value, out int val))
                {
                    comp_val = val;
                }
                else
                {
                    comp_val = Util.ParseFilesize(m.Groups["value"].Value);
                    if (comp_val == null)
                        comp_val = Util.ParseFilesize(m.Groups["value"].Value + "B");
                    if (comp_val == null)
                        throw new Exception("Invalid value " + m.Groups["value"].Value + " in format specification " + filter_spec);
                }
                op = (x, y) =>  operators[m.Groups["op"].Value]((int)x, (int)y);

            }
            else
            {
                var strOperators = new Dictionary<string, Func<string, string, bool>>
                {
                    { "=", (x, y) => x == y },
                    { "^=", (x, y) => x.StartsWith(y) },
                    { "$=", (x, y) => x.EndsWith(y) },
                    { "*=", (x, y) => x.Contains(y) },
                };

                string strOperator_rex = @"(?x)
                    \s*(?P<key>ext|acodec|vcodec|container|protocol|format_id)
                    \s*(?P<negation>!\s*)?(?P<op>" + string.Join("|", strOperators.Keys.Select(x => Regex.Escape(x))) +
                    @")(?P<none_inclusive>\s*\?)?
                    \s*(?P<value>[a-zA-Z0-9._-]+)
                    \s*$";

                m = Regex.Match(filter_spec, strOperator_rex);

                if (m.Success)
                {
                    comp_val = m.Groups["value"].Value;
                    var str_op = strOperators[m.Groups["op"].Value];
                    if (m.Groups["negation"].Success)
                    {
                        op = (x, y) => !str_op((string)x, (string)y);
                    }
                    else
                    {
                        op = (Func<object, object, bool>)str_op;
                    }
                }
            }

            if (!m.Success) throw new Exception("Invalid filter specification " + filter_spec);

            return new Func<IFormat, bool>(delegate (IFormat format) 
            {
                var actual_value = (string)null;
                if (actual_value == null) return m.Groups["none_inclusive"].Success;
                return op(actual_value, comp_val);
            });
        }

        public static IList<FormatSelector> ParseFormatSelection(string[] tokens, ref int index, bool inMerge = false, bool inPickfirst = false, bool inGroup = false)
        {
            List<FormatSelector> selectors = new List<FormatSelector>();
            FormatSelector current = null;
            for (index++; index < tokens.Length; index++)
            {
                if (tokens[index] == @")")
                {
                    if (!inGroup) index--;
                    break;
                }
                else if (inMerge && (tokens[index] == @"/" || tokens[index] == @","))
                {
                    index--;
                    break;
                }
                else if (inPickfirst && tokens[index] == @",")
                {
                    index--;
                    break;
                }
                else if (tokens[index] == @",")
                {
                    if (current == null) throw new Exception("\",\" must follow a format selector");
                    selectors.Add(current);
                    current = null;
                }
                else if (tokens[index] == @"/")
                {
                    if (current == null) throw new Exception("\"\\\" must follow a format selector");
                    var first_choice = current;
                    var second_choice = ParseFormatSelection(tokens, ref index, inPickfirst = true);
                    current = new FormatSelector(FormatType.Pickfirst, (first_choice, second_choice));
                }
                else if (tokens[index] == @"[")
                {
                    if (current == null) current = new FormatSelector(FormatType.Single, "best");
                    var filter = ParseFormatFilter(tokens, ref index);
                    current.Filter.Add(filter);
                }
                else if (tokens[index] == @"(")
                {
                    if (current != null) throw new Exception("Unexpected \"(\"");
                    var group = ParseFormatSelection(tokens, ref index, inGroup = true);
                    current = new FormatSelector(FormatType.Group, group);
                }
                else if (tokens[index] == @"+")
                {
                    var video_selector = current;
                    var audio_selector = ParseFormatSelection(tokens, ref index, inMerge = true).Single();
                    if (video_selector == null || audio_selector == null)
                        throw new Exception("\"+\" must be between two format selectors");
                    current = new FormatSelector(FormatType.Merge, (video_selector, audio_selector));
                }
                else current = new FormatSelector(FormatType.Single, tokens[index]);//throw new Exception("Operator not recognized: \"" + tokens[index] + "\"");
            }

            if (current != null)
            {
                selectors.Add(current);
            }
            return selectors;
        }

        public static Func<IList<IFormat>, IList<IFormat>> BuildSelectorFunction(IList<FormatSelector> selector, string mergeOutputFormat = null)
        {
            if (selector.Count == 1)
                return BuildSelectorFunction(selector.First(), mergeOutputFormat);

            var funcs = selector.Select(s => BuildSelectorFunction(s, mergeOutputFormat));
            return new Func<IList<IFormat>, IList<IFormat>>(delegate (IList<IFormat> ctx)
            {
                List<IFormat> formats = new List<IFormat>();
                foreach (var f in funcs)
                {
                    foreach (var fx in f(ctx))
                    {
                        formats.Add(fx);
                    }
                }
                return formats;
            });
        }

        public static Func<IList<IFormat>, IList<IFormat>> BuildSelectorFunction(FormatSelector selector, string mergeOutputFormat = null)
        {
            Func<IList<IFormat>, IList<IFormat>> selector_func = null;
            switch (selector.Type)
            {
                case FormatType.Group:
                    selector_func = BuildSelectorFunction((IList<FormatSelector>)selector.Selector, mergeOutputFormat);
                    break;
                case FormatType.Pickfirst:
                    var fs = (selector.Selector as List<FormatSelector>).Select(s => BuildSelectorFunction(s, mergeOutputFormat));
                    selector_func = new Func<IList<IFormat>, IList<IFormat>>(delegate (IList<IFormat> ctx)
                    {
                        foreach (var f in fs)
                        {
                            var picked_formats = f(ctx);
                            if (picked_formats != null)
                                return picked_formats;
                        }
                        return new List<IFormat>();
                    });
                    break;
                case FormatType.Single:
                    string format_spec = (string)selector.Selector;

                    selector_func = new Func<IList<IFormat>, IList<IFormat>>(delegate (IList<IFormat> formats)
                    {
                        if (formats == null) return new List<IFormat>();

                        List<IFormat> rformats = new List<IFormat>();

                        switch (format_spec)
                        {
                            case "all":
                                return formats;
                            case "best":
                            case null:
                                var best = formats.LastOrDefault(f => f is IMuxedFormat);
                                if (best == default) return null; else return new List<IFormat>() { best };
                            case "worst":
                                var worst = formats.FirstOrDefault(f => f is IMuxedFormat);
                                if (worst == default) return null; else return new List<IFormat>() { worst };
                            case "bestaudio":
                                var bestaudio = formats.LastOrDefault(f => f is IAudioFormat && !(f is IVideoFormat));
                                if (bestaudio == default) return null; else return new List<IFormat>() { bestaudio };
                            case "worstaudio":
                                var worstaudio = formats.FirstOrDefault(f => f is IAudioFormat && !(f is IVideoFormat));
                                if (worstaudio == default) return null; else return new List<IFormat>() { worstaudio };
                            case "bestvideo":
                                var bestvideo = formats.LastOrDefault(f => f is IVideoFormat && !(f is IAudioFormat));
                                if (bestvideo == default) return null; else return new List<IFormat>() { bestvideo };
                            case "worstvideo":
                                var worstvideo = formats.FirstOrDefault(f => f is IVideoFormat && !(f is IAudioFormat));
                                if (worstvideo == default) return null; else return new List<IFormat>() { worstvideo };
                            default:
                                Func<IFormat, bool> filter_f = null;
                                var extensions = new List<string> { "mp4", "flv", "webm", "3gp", "m4a", "mp3", "ogg", "aac", "wav" };
                                if (extensions.Contains(format_spec))
                                    filter_f = f => f.Extension == format_spec;
                                else
                                    filter_f = f => f.Id == format_spec;
                                var matches = formats.Where(filter_f);
                                if (matches != null && matches.Count() >= 1) return new List<IFormat> { matches.Last() };
                                break;
                        }
                        return new List<IFormat>();
                    });
                    break;
                case FormatType.Merge:
                    (FormatSelector, FormatSelector) formatTuple = ((FormatSelector, FormatSelector))selector.Selector;
                    var video_selector = BuildSelectorFunction(formatTuple.Item1, mergeOutputFormat);
                    var audio_selector = BuildSelectorFunction(formatTuple.Item2, mergeOutputFormat);

                    selector_func = new Func<IList<IFormat>, IList<IFormat>>(delegate (IList<IFormat> formats)
                    {
                        List<IFormat> retFormats = new List<IFormat>();
                        foreach (Tuple<IFormat, IFormat> tuple in Util.Product(
                            video_selector(formats), audio_selector(formats)))
                        {
                            IVideoFormat videoFormat = tuple.Item1 as IVideoFormat;
                            IAudioFormat audioFormat = tuple.Item2 as IAudioFormat;

                            if (videoFormat == null)
                                throw new Exception("The first format must contain the video, try using \"-f " + audioFormat.Id + "+" + videoFormat.Id + "\"");
                            if (audioFormat == null)
                                throw new Exception("Both formats " + videoFormat.Id + " and " + audioFormat.Id + " are video-only, you must specify \"-f video+audio\"");

                            CompFormat outf = new CompFormat(audioFormat, videoFormat);
                            if (mergeOutputFormat != null)
                                outf.Extension = mergeOutputFormat;
                            /*Format outf = new Format()
                            {
                                Id = videoFormat.Id + "+" + audioFormat.Id,
                                Name = videoFormat.Name + "+" + audioFormat.Name,
                                Width = videoFormat.Width,
                                Height = videoFormat.Height,
                                FPS = videoFormat.FPS,
                                VideoCodec = videoFormat.VideoCodec,
                                VideoBitrate = videoFormat.VideoBitrate,
                                StretchedRatio = videoFormat.StretchedRatio,
                                AudioCodec = audioFormat.AudioCodec,
                                AudioBitrate = audioFormat.AudioBitrate,
                                Extension = outputext
                            };*/
                            retFormats.Add(outf);
                        }
                        return retFormats;
                    });
                    break;
            }

            var filters = selector.Filter.Select(f => BuildFormatFilter(f));
            return new Func<IList<IFormat>, IList<IFormat>>(delegate (IList<IFormat> formats)
            {
                foreach (var filter in filters)
                {
                    formats = formats.Where(filter).ToList();
                }
                return selector_func(formats);
            });
        }
    }
}
