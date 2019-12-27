using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using YoutubeDL.Models;

namespace YoutubeDL.Downloaders
{
    public class HttpFD : FileDownloader
    {
        public override string[] Protocols => new string[] { "http", "https", "ftp" };
        public bool DoubleBufferCopy { get; set; } = false;
        public bool MultiThreadDownload { get; set; } = true;
        public int MaxThreads { get; set; } = Environment.ProcessorCount;
        public long DefaultChunkSize { get; set; } = 8000000;
        public int DefaultBufferSize { get; set; } = 100000;

        private static HttpClient client;
        public static HttpClient HttpClient
        {
            get
            {
                if (client == null)
                    client = new HttpClient();
                return client;
            }
            set
            {
                client = value;
            }
        }

        public HttpFD()
        {

        }

        public override void Download(IDownloadable format, string filename, bool overwrite = true)
        {
            base.Download(format, filename, overwrite);
            if (MultiThreadDownload)
                // Network work can only be done async
                MultiThreadedDownload(format, filename).GetAwaiter().GetResult();
            else
                SingleThreadedDownload(format, filename).GetAwaiter().GetResult();
        }

        public override async Task DownloadAsync(IDownloadable format, string filename, bool overwrite = true)
        {
            await base.DownloadAsync(format, filename, overwrite);
            if (MultiThreadDownload)
                await MultiThreadedDownload(format, filename).ConfigureAwait(false);
            else
                await SingleThreadedDownload(format, filename).ConfigureAwait(false);
        }

        public async Task SingleThreadedDownload(IDownloadable format, string filename)
            => await SingleThreadedDownload(format.Url, filename, format.HttpHeaders, (int)format.DownloaderOptions.GetValueOrDefault("http_chunk_size"));

        public async Task SingleThreadedDownload(string url, string filename, Dictionary<string, string> headers = null, int? chunkSize = null)
        {
            startTime = DateTime.Now;
            long current = 0;

            long chunksize;
            if (chunkSize.HasValue && chunkSize != default)
                chunksize = chunkSize.Value;
            else
                chunksize = DefaultChunkSize;

            using FileStream f = File.OpenWrite(filename);
            while (true)
            {
                HttpRequestMessage message = new HttpRequestMessage(HttpMethod.Get, url);

                if (headers != null)
                    foreach (var h in headers)
                        message.Headers.Add(h.Key, h.Value);

                message.Headers.Range = new System.Net.Http.Headers.RangeHeaderValue(current, current + chunksize - 1);

                var resp = await HttpClient.SendAsync(message, HttpCompletionOption.ResponseHeadersRead);

                if (resp.Headers.TransferEncodingChunked.HasValue &&
                    resp.Headers.TransferEncodingChunked.Value)
                {
                    f.Seek(0, SeekOrigin.Begin);
                    using (Stream s = await resp.Content.ReadAsStreamAsync())
                    {
                        if (DoubleBufferCopy) //await s.DoubleBufferCopyToAsync(f, DefaultBufferSize);
                            await s.DoubleBufferCopyToAsync(f, DefaultBufferSize, (rec, s) => RaiseProgress(rec, -1));
                        else //await s.CopyToAsync(f, DefaultBufferSize);
                            await s.CopyToAsync(f, DefaultBufferSize, (rec, s) => RaiseProgress(rec, -1));
                    }
                    RaiseProgress(f.Length, f.Length);
                    break;
                }

                GetContentRange(resp, out long from, out long to, out long total);
                f.Seek((int)from, SeekOrigin.Begin);

                using (Stream s = await resp.Content.ReadAsStreamAsync())
                {
                    if (DoubleBufferCopy) //await s.DoubleBufferCopyToAsync(f, DefaultBufferSize);
                        await s.DoubleBufferCopyToAsync(f, DefaultBufferSize, (rec, s) => RaiseProgress(rec, total));
                    else //await s.CopyToAsync(f, DefaultBufferSize);
                        await s.CopyToAsync(f, DefaultBufferSize, (rec, s) => RaiseProgress(rec, total));
                }

                //RaiseProgress(to + 1, total);

                if (f.Length < total)
                {
                    current = f.Length - 1;
                    continue;
                }
                else break;
            }
        }

        public Task MultiThreadedDownload(IDownloadable format, string filename)
            => MultiThreadedDownload(format.Url, filename, format.HttpHeaders, (int)format.DownloaderOptions.GetValueOrDefault("http_chunk_size"));

        public async Task MultiThreadedDownload(string url, string filename, Dictionary<string,string> headers = null, int? chunkSize = null)
        {
            startTime = DateTime.Now;
            HttpRequestMessage imessage = new HttpRequestMessage(HttpMethod.Head, url);

            long chunksize;
            if (chunkSize.HasValue && chunkSize != default)
                chunksize = chunkSize.Value;
            else
                chunksize = DefaultChunkSize;

            if (headers != null)
                foreach (var h in headers)
                    imessage.Headers.Add(h.Key, h.Value);

            var iresp = await HttpClient.SendAsync(imessage, HttpCompletionOption.ResponseHeadersRead);

            long total = iresp.Content.Headers.ContentLength.Value;
            List<Task> readTasks = new List<Task>();


            var loops = (int)Math.Ceiling((double)total / chunksize);
            int[] nums = new int[loops];
            for (int i = 0; i < loops; i++) nums[i] = i;

            long recievedTotal = 0;

            await Util.ParallelForEachAsync(nums, (l) =>
            {
                var current = (l * chunksize);
                HttpRequestMessage message = new HttpRequestMessage(HttpMethod.Get, url);

                message.Headers.Range = new System.Net.Http.Headers.RangeHeaderValue(current, current + chunksize - 1);
                if (headers != null)
                    foreach (var h in headers)
                        imessage.Headers.Add(h.Key, h.Value);

                return ContentToFileAsync(filename, message, (recievedx, sizex) => { recievedTotal += sizex;RaiseProgress(recievedTotal, total); });//.ContinueWith((u) => RaiseProgress(l * , loops)); //}).ContinueWith((u) => RaiseProgress(recievedTotal, total)); ;//RaiseProgress(recieved, total); });//.ContinueWith((u) => RaiseProgress(l * , loops));
            }, MaxThreads);
        }

        protected void GetContentRange(HttpResponseMessage resp, out long from, out long to, out long total)
        {
            if (resp.Content.Headers.Contains("Content-Range"))
            {
                var total1 = resp.Content.Headers.ContentRange.Length;

                var from1 = resp.Content.Headers.ContentRange.From;
                var to1 = resp.Content.Headers.ContentRange.To;

                if (total1.HasValue && from1.HasValue && to1.HasValue)
                {
                    total = total1.Value;
                    from = from1.Value;
                    to = to1.Value;
                }
                else
                {
                    total = resp.Content.Headers.ContentLength.Value;
                    from = 0;
                    to = total - 1;
                }
            }
            else
            {
                total = resp.Content.Headers.ContentLength.Value;
                from = 0;
                to = total - 1;
            }
        }
        protected async Task<long> ContentToStreamAsync(Stream f, HttpRequestMessage message, Action<long, int> onProgress = null)
        {
            var resp = await HttpClient.SendAsync(message, HttpCompletionOption.ResponseHeadersRead);

            if (resp.Headers.TransferEncodingChunked.HasValue &&
                    resp.Headers.TransferEncodingChunked.Value)
            {
                f.Seek(0, SeekOrigin.Begin);
                using (Stream s = await resp.Content.ReadAsStreamAsync())
                {
                    if (DoubleBufferCopy)
                        await s.DoubleBufferCopyToAsync(f, DefaultBufferSize, (rec, s) => onProgress(rec, -1));
                    else
                        await s.CopyToAsync(f, DefaultBufferSize, (rec, s) => onProgress(rec, -1));
                }
                onProgress(f.Length, (int)f.Length);
                return f.Length;
            }
            else
            {
                GetContentRange(resp, out long from, out long to, out long total);

                using Stream s = await resp.Content.ReadAsStreamAsync();
                f.Seek(from, SeekOrigin.Begin);

                if (DoubleBufferCopy)
                    await s.DoubleBufferCopyToAsync(f, DefaultBufferSize, onProgress);
                else
                    await s.CopyToAsync(f, DefaultBufferSize, onProgress);
                return to;
            }
        }

        protected async Task ContentToFileAsync(string filename, HttpRequestMessage message, Action<long, int> onProgress = null)
        {
            using FileStream f = File.Open(filename, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Write);
            await ContentToStreamAsync(f, message, onProgress);
        }
    }
}
