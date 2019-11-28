using System;
using System.Collections.Generic;
using System.Text;

namespace YoutubeDL.Models
{
    public class Subtitle : InfoDict
    {
        public string Name { get; set; }
        public List<SubtitleFormat> Formats { get; set; }

        public Subtitle(string name) : base()
        {
            Name = name;
        }
    }

    public class SubtitleFormat : InfoDict
    {
        [YTDLMeta("url")]
        public string Url { get; set; }
        [YTDLMeta("ext")]
        public string Extension { get; set; }

        public SubtitleFormat() : base()
        {

        }
        public SubtitleFormat(Dictionary<string, object> infoDict) : base(infoDict)
        {

        }
    }
}
