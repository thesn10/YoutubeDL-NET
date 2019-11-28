using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Http;

namespace YoutubeDL
{
    /// <summary>
    /// Classes that use this interface are able to manage InfoExtractors
    /// </summary>
    public interface IManagingDL
    {
        public YoutubeDLOptions Options { get; set; }
        public HttpClient HttpClient { get; }
    }

    interface IDLOptions
    {

    }
}
