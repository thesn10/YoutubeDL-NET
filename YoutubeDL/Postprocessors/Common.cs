using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using YoutubeDL.Models;

namespace YoutubeDL.Postprocessors
{
    public interface IPostProcessor
    {

    }

    public interface IPostProcessor<T> : IPostProcessor, IHasLog, IHasProgress where T : IFormat
    {
        public void Process(T format, string filename);
        public Task ProcessAsync(T format, string filename, CancellationToken token = default);
    }

    public class PostProcessor
    {
        #region Logging
        public event LogEventHandler OnLog;
        protected void LogDebug(string message, string sender = null, bool writeline = true, string colormessage = null)
            => Log(message, LogType.Debug, sender, writeline, colormessage);
        protected void LogInfo(string message, string sender = null, bool writeline = true, string colormessage = null)
            => Log(message, LogType.Info, sender, writeline, colormessage);
        protected void LogWarning(string message, string sender = null, bool writeline = true, string colormessage = null)
            => Log(message, LogType.Warning, sender, writeline, colormessage);
        protected void LogError(string message, string sender = null, bool writeline = true, string colormessage = null)
            => Log(message, LogType.Error, sender, writeline, colormessage);

        protected void Log(string message, LogType type, string sender = null, bool writeline = true, string colormessage = null, bool ytdlpy = false)
        {
            string[] sarr = null;
            if (sender != null) sarr = new string[] { sender };
            Log(message, type, sarr, writeline, colormessage, ytdlpy);
        }

        protected void Log(string message, LogType type, string[] sender = null, bool writeline = true, string colormessage = null, bool ytdlpy = false)
        {
            var args = Logger.Instance.Log(message, type, sender, writeline, colormessage, ytdlpy);
            Log(this, args);
        }

        protected void Log(object sender, LogEventArgs e) => OnLog?.Invoke(this, e);
        #endregion
    }
}
