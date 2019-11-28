using System;
using System.Collections.Generic;
using System.Text;

namespace YoutubeDL
{
    /// <summary>
    /// This Attribute indicates what the equivalent name in the youtube-dl python version is.
    /// </summary>
    public sealed class YTDLMetaAttribute : Attribute
    {
        public string PythonName { get; set; }
        public string ArgName { get; set; }
        public string ShortArgName { get; set; }
        public string Description { get; set; }
        public YTDLMetaAttribute(string pythonName, string shortArgName = null)
        {
            this.PythonName = pythonName;
            this.ArgName = pythonName.Replace("_", "-");
            this.ShortArgName = shortArgName;
        }
    }
}
