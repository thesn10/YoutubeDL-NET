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
        YoutubeDLOptions Options { get; set; }
        HttpClient HttpClient { get; }
    }

    interface IDLOptions
    {

    }
}
