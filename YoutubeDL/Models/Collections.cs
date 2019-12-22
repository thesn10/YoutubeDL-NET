using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;

namespace YoutubeDL.Models
{
    public static class FormatCollectionExt
    {
        public static IList<IFormat> SelectFormats(this IList<IFormat> formats, string format_spec, string mergeOutputFormat = null)
            => FormatParser.SelectFormats(formats, format_spec, mergeOutputFormat);
        public static IEnumerable<IAudioFormat> WithAudio(this IEnumerable<IFormat> formats) => formats.Where(x => x is IAudioFormat).Select(x => (IAudioFormat)x);
        public static IEnumerable<IVideoFormat> WithVideo(this IEnumerable<IFormat> formats) => formats.Where(x => x is IVideoFormat).Select(x => (IVideoFormat)x);
        public static IEnumerable<IAudioFormat> GetAudioOnlyFormats(this IEnumerable<IFormat> formats) => formats.WithAudio().Where(x => !(x is IVideoFormat));
        public static IEnumerable<IVideoFormat> GetVideoOnlyFormats(this IEnumerable<IFormat> formats) => formats.WithVideo().Where(x => !(x is IAudioFormat));
        public static IEnumerable<IMuxedFormat> GetMuxedFormats(this IEnumerable<IFormat> formats) => formats.Where(x => x is IMuxedFormat).Select(x => (IMuxedFormat)x);

        public static IMuxedFormat MuxedWithBestResolution(this IEnumerable<IFormat> formats) => formats.GetMuxedFormats().OrderByDescending(x => x.Height).First();
        public static IVideoFormat WithBestVideoResolution(this IEnumerable<IFormat> formats) => formats.WithVideo().OrderByDescending(x => x.Height).First();
        public static IAudioFormat WithBestAudioBitrate(this IEnumerable<IFormat> formats) => formats.WithAudio().OrderByDescending(x => x.AudioBitrate).First();
    }

    public class FormatCollection : ReadOnlyCollection<IFormat>
    {
        public FormatCollection(IList<IFormat> formats) : base(formats)
        {
        }

        public FormatCollection(IEnumerable<IFormat> formats) : base(formats.ToList())
        {
        }

        public IList<IFormat> SelectFormats(string format_spec, string mergeOutputFormat = null)
            => FormatParser.SelectFormats(Items, format_spec, mergeOutputFormat);

        public IEnumerable<IAudioFormat> WithAudio() => Items.Where(x => x is IAudioFormat).Select(x => (IAudioFormat)x);
        public IEnumerable<IVideoFormat> WithVideo() => Items.Where(x => x is IVideoFormat).Select(x => (IVideoFormat)x);
        public IEnumerable<IAudioFormat> GetAudioOnlyFormats() => WithAudio().Where(x => !(x is IVideoFormat));
        public IEnumerable<IVideoFormat> GetVideoOnlyFormats() => WithVideo().Where(x => !(x is IAudioFormat));
        public IEnumerable<IMuxedFormat> GetMuxedFormats() => Items.Where(x => x is IMuxedFormat).Select(x => (IMuxedFormat)x);

        public IMuxedFormat MuxedWithBestResolution() => GetMuxedFormats().OrderByDescending(x => x.Height).First();
        public IVideoFormat WithBestVideoResolution() => WithVideo().OrderByDescending(x => x.Height).First();
        public IAudioFormat WithBestAudioBitrate() => WithAudio().OrderByDescending(x => x.AudioBitrate).First();
    }

    public class ThumbnailCollection : ReadOnlyCollection<Thumbnail>
    {
        public ThumbnailCollection(IList<Thumbnail> list) : base(list)
        {
        }

        public Thumbnail WithBestResolution() => Items.OrderByDescending(x => x.Height).First();
    }

    public class SubtitleCollection : ReadOnlyCollection<Subtitle>
    {
        public SubtitleCollection(IList<Subtitle> list) : base(list)
        {
        }

        public Subtitle WithSystemLanguage() => WithLanguage(CultureInfo.CurrentCulture);
        public Subtitle WithLanguage(string languageCode) => Items.First(x => x.Name == languageCode);
        public Subtitle WithLanguage(CultureInfo culture) => Items.First(x => x.Name == culture.TwoLetterISOLanguageName);
    }
}
