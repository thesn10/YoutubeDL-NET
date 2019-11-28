using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Http.Headers;
using System.IO;

namespace YoutubeDL
{
    internal class YTDL_HttpClientHandler : HttpClientHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (request.RequestUri.IsFile)
            {
                throw new Exception(@"file:// scheme is explicitly disabled in youtube-dl for security reasons");
            }
            return base.SendAsync(request, cancellationToken);
        }
    }
}
