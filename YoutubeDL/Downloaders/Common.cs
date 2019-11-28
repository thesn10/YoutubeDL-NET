using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using YoutubeDL.Models;
using YoutubeDL;

namespace YoutubeDL.Downloaders
{
    public interface IFileDownloader : IHasLog, IHasProgress
    {
        public string[] Protocols { get; }
        public void Download(IDownloadable format, string filename, bool overwrite = true);
        public Task DownloadAsync(IDownloadable format, string filename, bool overwrite = true);
    }

    public abstract class FileDownloader : IFileDownloader
    {
        public event ProgressEventHandler OnProgress;
        public event LogEventHandler OnLog;

        protected void RaiseProgress(ProgressEventArgs e) 
            => OnProgress?.Invoke(this, e);
        protected void RaiseProgress(long recieved, long total)
            => RaiseProgress(new ProgressEventArgs(recieved, total, "B", startTime));

        protected DateTime startTime;

        public abstract string[] Protocols { get; }

        public static FileDownloader GetSuitableDownloader(string protocol, bool hls_prefer_native = false)
        {
            // maybe make this a property of the filedownloaders?
            if (protocol == "https" ||
                protocol == "http" ||
                protocol == "ftp")
            {
                return GetDownloader<HttpFD>();
            }

            // todo
            if (hls_prefer_native)
            {
                //todo
            }

            return null;
        }

        public static T GetDownloader<T>() where T : FileDownloader
        {
            return (T)typeof(T).GetConstructor(new Type[] { }).Invoke(null);
                //.GetProperty("Downloader", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public)
                //.GetValue(null);
        }

        public virtual void Download(IDownloadable format, string filename, bool overwrite = true)
        {
            if (File.Exists(filename) && overwrite == false) return;
            startTime = DateTime.Now;
        }

        public virtual async Task DownloadAsync(IDownloadable format, string filename, bool overwrite = true)
        {
            if (File.Exists(filename) && overwrite == false) return;
            startTime = DateTime.Now;
        }
    }
}
