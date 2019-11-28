using System;
using System.Collections.Generic;
using System.Text;

namespace YoutubeDL.Models
{
    public class Playlist : InfoDict
    {
        [YTDLMeta("id")]
        public string Id { get; set; }
        [YTDLMeta("title")]
        public string Title { get; set; }
        public string Uploader { get; set; }
        public string UploaderId { get; set; }
        public string PlaylistSize { get; set; }
        public List<InfoDict> Entries { get; set; }
        public Playlist() : base()
        {

        }

        public Playlist(Dictionary<string, object> infoDict) : base(infoDict)
        {
            if (infoDict.TryGetValue("entries", out object entries))
            {
                Entries = new List<InfoDict>();
                List<object> xentries = (List<object>)entries;
                foreach (Dictionary<string, object> entryDict in xentries)
                {
                    var entryInfoDict = InfoDict.FromDict(entryDict);
                    Entries.Add(entryInfoDict);
                }
            }
        }
    }
}
