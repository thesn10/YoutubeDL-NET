using System;

namespace YoutubeDL
{
    /// <summary>
    /// This Attribute stores metadata of properties for youtube-dl
    /// </summary>
    public sealed class YTDLMetaAttribute : Attribute
    {
        /// <summary>
        /// The equivalent name in the youtube-dl python version (for auto-fill)
        /// </summary>
        public string PythonName { get; set; }

        /// <summary>
        /// Command line argument name (ex: --download)
        /// </summary>
        public string ArgName { get; set; }

        /// <summary>
        /// Short command line argument name (ex: -d)
        /// </summary>
        public string ShortArgName { get; set; }

        /// <summary>
        /// A description that is shown by the "help" command/function
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Controls if the value is assicgned automatically if the names match or manually by corresponding model constructor. 
        /// </summary>
        public bool AutoFill { get; set; } = true;

        public YTDLMetaAttribute(string pythonName, string shortArgName = null)
        {
            this.PythonName = pythonName;
            this.ArgName = pythonName.Replace("_", "-");
            this.ShortArgName = shortArgName;
        }
    }
}
