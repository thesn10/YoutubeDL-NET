using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Reflection;
using System.Net;
using System.Net.Http;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using YoutubeDL.Downloaders;
using System.Threading.Tasks;
using YoutubeDL.Models;

namespace YoutubeDL.Extractors
{
    public interface IInfoExtractor : IHasLog
    {
        string Name { get; }
        string Description { get; }
        bool Working { get; }
        // IE Key
        bool Suitable(string url);
        InfoDict Extract(string url);
        Task<InfoDict> ExtractAsync(string url);
        void Initialize();
    }

    /// <summary>
    /// A base class for info extractors that provides basic logging and networking capabilities
    /// </summary>
    public abstract class InfoExtractor : IInfoExtractor
    {
        public abstract string Name { get; }
        public abstract string Description { get; }
        public abstract bool Working { get; }
        public virtual Regex MatchRegex { get; set; }
        public abstract InfoDict Extract(string url);
        public abstract Task<InfoDict> ExtractAsync(string url);
        public IManagingDL YoutubeDL { get; set; }


        private HttpClient httpClient = null;
        protected HttpClient HttpClient
        {
            get
            {
                if (YoutubeDL != null)
                {
                    return YoutubeDL.HttpClient;
                }
                else if (httpClient == null)
                {
                    httpClient = new HttpClient();
                }
                return httpClient;
            }
        }
        private YoutubeDLOptions options = null;

        protected YoutubeDLOptions Options
        {
            get
            {
                if (YoutubeDL != null)
                {
                    return YoutubeDL.Options;
                }
                else if (options == null)
                {
                    options = new YoutubeDLOptions();
                }
                return options;
            }
        }

        public InfoExtractor()
        {

        }

        public InfoExtractor(IManagingDL dl)
        {
            YoutubeDL = dl;
        }

        public virtual bool Suitable(string url)
        {
            if (MatchRegex == null)
                throw new ExtractorException("Extractor has not defined Suitable() or specified a MatchRegex");
            return MatchRegex.IsMatch(url);
        }

        public abstract void Initialize();

        protected async Task<HttpResponseMessage> RequestWebpage(HttpRequestMessage request, string video_id, string note = null, string errnote = null, bool fatal = true, HttpStatusCode[] expected_status = null)
        {
            if (note == null)
            {
                note = video_id + ": Downloading webpage";
            }
            LogInfo(note);

            // geo bypass
            string xForwardedFor = null; // todo 
            if (xForwardedFor != null && !request.Headers.Contains("X-Forwarded-For"))
            {
                request.Headers.Add("X-Forwarded-For", xForwardedFor);
            }

            try
            {
                var response = await HttpClient.SendAsync(request).ConfigureAwait(false);
                if (expected_status != null)
                {
                    if (!expected_status.Contains(response.StatusCode))
                    {
                        if (errnote == null)
                        {
                            errnote = video_id + ": Unable to download webpage";
                        }
                        LogInfo(errnote);

                        if (fatal)
                        {
                            throw new Exception(errnote);
                        }

                        return null;
                    }
                }
                return response;
            }
            catch (Exception ex)
            {
                if (errnote == null)
                {
                    errnote = video_id + ": Unable to download webpage";
                }
                LogInfo(errnote);

                if (fatal)
                {
                    throw new Exception(errnote, ex);
                }
                return null;
            }
        }

        protected async Task<string> DownloadWebpage(string uri)
        {
            return await HttpClient.GetStringAsync(uri).ConfigureAwait(false);
        }

        protected async Task<string> DownloadWebpage(Uri uri)
        {
            return await HttpClient.GetStringAsync(uri).ConfigureAwait(false);
        }

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

    /// <summary>
    /// A base class for info extractors that are very simple and dont need a network client (like invoking another IE or analyzing the url)
    /// </summary>
    public abstract class SimpleInfoExtractor : IInfoExtractor
    {
        public abstract string Name { get; }
        public abstract string Description { get; }
        public abstract bool Working { get; }
        protected virtual Regex MatchRegex { get; }

        public event LogEventHandler OnLog;

        public abstract InfoDict Extract(string url);
        public abstract Task<InfoDict> ExtractAsync(string url);
        public abstract void Initialize();

        public virtual bool Suitable(string url)
        {
            if (MatchRegex == null)
                throw new ExtractorException("Extractor has not defined Suitable() or specified a MatchRegex");
            return MatchRegex.IsMatch(url);
        }
    }

    /// <summary>
    /// A base class for InfoExtractors that implement multiple methods to exract multiple types of info
    /// </summary>
    public abstract class MultiInfoExtractor : InfoExtractor
    {
        public override abstract bool Working { get; }
        public override abstract string Description { get; }
        public override abstract string Name { get; }

        public MultiInfoExtractor(IManagingDL dl) : base(dl)
        {
        }

        public MultiInfoExtractor()
        {
        }

        public override bool Suitable(string url)
        {
            var methods = this.GetType().GetMethods();
            foreach (var method in methods)
            {
                var attr = method.GetCustomAttribute<ExtractionFuncAttribute>();
                if (attr == null) continue;
                if (attr.ValidUrlRegex.IsMatch(url))
                    return true;
            }
            return false;
        }

        public override InfoDict Extract(string url)
        {
            var methods = this.GetType().GetMethods();
            foreach (var method in methods)
            {
                var attr = method.GetCustomAttribute<ExtractionFuncAttribute>();
                if (attr == null) continue;
                if (!attr.ValidUrlRegex.IsMatch(url)) continue;

                if (!attr.Working) 
                {
                    LogWarning("The program functionality for this site has been marked as broken, and will probably not work.", this.GetType().Name);
                }

                if (method.ReturnType.IsSubclassOf(typeof(Task)))
                {
                    return ((Task<InfoDict>)method.Invoke(this, new object[] { url })).GetAwaiter().GetResult();
                }
                else if (method.ReturnType.IsSubclassOf(typeof(InfoDict)))
                {
                    return (InfoDict)method.Invoke(this, new object[] { url });
                }
            }
            return null;
        }

        public override async Task<InfoDict> ExtractAsync(string url)
        {
            var methods = this.GetType().GetMethods();
            foreach (var method in methods)
            {
                var attr = method.GetCustomAttribute<ExtractionFuncAttribute>();
                if (attr == null) continue;
                if (!attr.ValidUrlRegex.IsMatch(url)) continue;

                if (!attr.Working)
                {
                    LogWarning("The program functionality for this site has been marked as broken, and will probably not work.", this.GetType().Name);
                }

                if (method.ReturnType.IsSubclassOf(typeof(Task)))
                {
                    return await ((Task<InfoDict>)method.Invoke(this, new object[] { url })).ConfigureAwait(false);
                }
                else if (method.ReturnType.IsSubclassOf(typeof(InfoDict)))
                {
                    return (InfoDict)method.Invoke(this, new object[] { url });
                }
            }
            return null;
        }

        public override abstract void Initialize();
    }

    public class ExtractionFuncAttribute : Attribute
    {
        public string ValidUrl { get; set; }
        private Regex validUrlRegex;
        public Regex ValidUrlRegex
        {
            get
            {
                if (validUrlRegex == null)
                    validUrlRegex = new Regex(ValidUrl);
                return validUrlRegex;
            }
        }
        public bool Working { get; set; }

        public ExtractionFuncAttribute(string regex)
        {
            ValidUrl = regex;
        }
        public ExtractionFuncAttribute(string regex, bool working)
        {
            ValidUrl = regex;
            Working = working;
        }
    }
}
