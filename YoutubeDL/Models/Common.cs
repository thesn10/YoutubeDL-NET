using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;

namespace YoutubeDL.Models
{
    /// <summary>
    /// A base class for models. Automatically maps python dicts to the model subclass. 
    /// </summary>
    public partial class InfoDict
    {
        [YTDLMeta("extractor")]
        public string Extractor { get; set; }
        [YTDLMeta("extractor_key")]
        public string ExtractorKey { get; set; }
        [YTDLMeta("webpage_url")]
        public string WebpageUrl { get; set; }
        [YTDLMeta("webpage_url_basename")]
        public string WebpageUrlBasename { get; set; }

        public Dictionary<string, object> AdditionalProperties { get; set; } = new Dictionary<string, object>();

        public InfoDict() : base()
        {

        }

        internal InfoDict(Dictionary<string, object> infoDict)
        {
            AddExtraInfo(infoDict);
        }

        public static T FromDict<T>(Dictionary<string, object> infoDict, bool ooo) where T : InfoDict
        {
            return (T)typeof(T)
                .GetConstructors(BindingFlags.NonPublic)
                .First(x => x
                    .GetParameters().Length == 1 && x
                    .GetParameters()
                    .Single().ParameterType == typeof(Dictionary<string, object>))
                .Invoke(new object[] { infoDict });
        }

        public static T FromDict<T>(Dictionary<string, object> infoDict) where T : InfoDict
        {
            Type t = typeof(T);
            var constructors = t.GetConstructors();
            var con = constructors.First(x => x
                    .GetParameters().Length == 1 && x
                    .GetParameters()
                    .Single().ParameterType == typeof(Dictionary<string, object>));
            return (T)con.Invoke(new object[] { infoDict });
        }

        public static InfoDict FromDict(Dictionary<string, object> infoDict, string type = null)
        {
            if (type == null)
                type = (string)infoDict.GetValueOrDefault("_type");

            if (infoDict.ContainsKey("_type")) infoDict.Remove("_type");

            switch (type)
            {
                case "video":
                    return new Video(infoDict);
                case "playlist":
                    return new Playlist(infoDict);
                case "url":
                    return new ContentUrl(infoDict);
                case "url-transparent":
                    return new TransparentUrl(infoDict);
                case "format":
                    return FormatBase.FromDict(infoDict);
                case "thumbnail":
                    return new Thumbnail(infoDict);
                case "subtitleformat":
                    return new SubtitleFormat(infoDict);
                default:
                    return new InfoDict(infoDict);
            }
        }

        public void AddExtraInfo(Dictionary<string, object> extraInfo, bool overwrite = true)
        {
            if (extraInfo == null) return;

            foreach (var kv in extraInfo)
            {
                var pInfo = this.GetType()
                    .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                    .Where(delegate (PropertyInfo p)
                    {
                        var attr = (p.GetCustomAttribute(typeof(YTDLMetaAttribute)) as YTDLMetaAttribute);
                        return attr?.PythonName == kv.Key && attr.AutoFill;
                    })
                    .FirstOrDefault();

                if (pInfo != default)
                {
                    if (pInfo.GetValue(this) != null && !overwrite) continue;
                    object val = pInfo.PropertyType == typeof(int) || pInfo.PropertyType == typeof(int?) ? Convert.ToInt32(kv.Value) : kv.Value; // long to int
                    pInfo.SetValue(this, val);
                }
                else
                {
                    var fInfo = this.GetType()
                        .GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                        .Where(delegate (FieldInfo p)
                        {
                            var attr = (p.GetCustomAttribute(typeof(YTDLMetaAttribute)) as YTDLMetaAttribute);
                            return attr?.PythonName == kv.Key && attr.AutoFill;
                        })
                        .FirstOrDefault();

                    if (fInfo != default)
                    {
                        if (fInfo.GetValue(this) != null && !overwrite) continue;
                        object val = fInfo.FieldType == typeof(int) || fInfo.FieldType == typeof(int?) ? Convert.ToInt32(kv.Value) : kv.Value; // long to int
                        fInfo.SetValue(this, val);
                    }
                    else if (AdditionalProperties.ContainsKey(kv.Key))
                    {
                        if (overwrite) AdditionalProperties[kv.Key] = kv.Value;
                    }
                    else
                    {
                        AdditionalProperties.Add(kv.Key, kv.Value);
                    }
                }
            }
        }
    }

    public interface IDownloadable
    {
        bool IsDownloaded { get; set; }
        string FileName { get; set; }
        string Url { get; set; }
        Dictionary<string, string> HttpHeaders { get; set; }
        Dictionary<string, object> DownloaderOptions { get; set; }
    }
}
