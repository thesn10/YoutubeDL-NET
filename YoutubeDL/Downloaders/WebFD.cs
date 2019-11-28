using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Http;
using System.Linq;
using YoutubeDL.Models;
using System.Threading.Tasks;

namespace YoutubeDL.Downloaders
{
    /// <summary>
    /// This class uses <see cref="WebClient"/> which is singlethreaded, and therefore slow af (good work microsoft!)
    /// </summary>
    [Obsolete]
    public class WebFD : FileDownloader
    {
        private readonly WebClient client;

        public override string[] Protocols => new string[] { "http", "https", "ftp" };

        public WebFD()
        {
            client = new WebClient();
            client.DownloadProgressChanged += Client_DownloadProgressChanged;
        }

        private void Client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            RaiseProgress(e.BytesReceived, e.TotalBytesToReceive);
        }

        public async Task DownloadAsync(string url, string filename, Dictionary<string,string> headers = null)
        {
            if (headers != null)
                foreach (var h in headers)
                    client.Headers.Add(h.Key, h.Value);

            await client.DownloadFileTaskAsync(new Uri(url), filename);
        }

        public void Download(string url, string filename, Dictionary<string, string> headers = null)
        {
            if (headers != null)
                foreach (var h in headers)
                    client.Headers.Add(h.Key, h.Value);

            client.DownloadFile(new Uri(url), filename);
        }

        public override void Download(IDownloadable format, string filename, bool overwrite = true)
        {
            base.Download(format, filename, overwrite);
            Download(format.Url, filename, format.HttpHeaders);
        }

        public override async Task DownloadAsync(IDownloadable format, string filename, bool overwrite = true) 
        {
            await base.DownloadAsync(format, filename, overwrite);
            await DownloadAsync(format.Url, filename, format.HttpHeaders);
        }
    }
}
