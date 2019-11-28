using System;
using System.Collections.Generic;
using System.Text;

namespace YoutubeDL.Models
{
    public class ContentUrl : InfoDict
    {
        [YTDLMeta("url")]
        public string Url { get; set; }
        [YTDLMeta("ie_key")]
        public string IEKey { get; set; }
        public ContentUrl() : base()
        {

        }

        public ContentUrl(Dictionary<string, object> infoDict) : base(infoDict)
        {

        }
    }

    public class TransparentUrl : ContentUrl
    {
        public TransparentUrl() : base()
        {

        }

        public TransparentUrl(Dictionary<string, object> infoDict) : base(infoDict)
        {

        }
    }
}
