using System;
using System.Collections.Generic;
using System.Text;

namespace YoutubeDL.Models
{
    public class Thumbnail : InfoDict, IComparable
    {
        [YTDLMeta("id")]
        public string Id { get; set; }
        [YTDLMeta("url")]
        public string Url { get; set; }
        [YTDLMeta("preference")]
        public string Preference { get; set; }
        [YTDLMeta("width")]
        public int Width { get; set; }
        [YTDLMeta("height")]
        public int Height { get; set; }
        public Thumbnail() : base()
        {

        }
        public Thumbnail(Dictionary<string, object> infoDict) : base(infoDict)
        {

        }

        public int CompareTo(object obj)
        {
            Thumbnail cmp = obj as Thumbnail;
            if (cmp == null) return 1;

            if (Preference != null && Preference.CompareTo(cmp.Preference) != 0)
            {
                return Preference.CompareTo(cmp.Preference);
            }
            else if (Width.CompareTo(cmp.Width) != 0)
            {
                return Width.CompareTo(cmp.Width);
            }
            else if (Height.CompareTo(cmp.Height) != 0)
            {
                return Height.CompareTo(cmp.Height);
            }
            else if (Id != null && Id.CompareTo(cmp.Id) != 0)
            {
                return Id.CompareTo(cmp.Id);
            }
            else
            {
                return Url.CompareTo(Url);
            }
        }
    }
}
