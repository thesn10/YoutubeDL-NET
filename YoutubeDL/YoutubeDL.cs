using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Net.Http;
using System.IO;
using System.Text.RegularExpressions;
using System.Net;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using System.Text.Json;

using YoutubeDL.Downloaders;
using YoutubeDL.Extractors;
using YoutubeDL.Postprocessors;
using YoutubeDL.Models;
using System.Diagnostics;
using System.Threading;

[assembly: InternalsVisibleTo("YoutubeDL.Python")]
namespace YoutubeDL
{
    // would call YoutubeDL but namespace is already called like this :(
    public partial class YouTubeDL : IManagingDL, IDisposable
    {
        public YouTubeDL()
        {
            Options = new YoutubeDLOptions();
            AddExtractors();
        }
        public YouTubeDL(YoutubeDLOptions settings)
        {
            Options = settings;
            AddExtractors();
        }

        public YoutubeDLOptions Options { get; set; }
        public int NumDownloads { get; private set; } = 0;

        public Func<CompFormat, IPostProcessor<CompFormat>> Merger { get; set; } = (f) => new FFMpegMergerPP(true);
        public Func<IFormat, IPostProcessor> Converter { get; set; } = (f) => new FFMpegConverterPP("");

        internal readonly List<IInfoExtractor> ie_instances = new List<IInfoExtractor>();

        protected HttpClient httpClient;
        private YTDL_HttpClientHandler httpClientHandler;
        public HttpClient HttpClient
        { 
            get 
            { 
                if (httpClient == null) SetupHttpClient();
                return httpClient;
            } 
        }

        internal YTDL_HttpClientHandler HttpClientHandler
        {
            get
            {
                if (httpClientHandler == null) SetupHttpClient();
                return httpClientHandler;
            }
        }

        private static readonly object ConsoleWriterLock = new object();

        #region Logging
        public event LogEventHandler OnLog;
        internal void LogDebug(string message, string sender = null, bool writeline = true, string colormessage = null)
            => Log(message, LogType.Debug, sender, writeline, colormessage);
        internal void LogInfo(string message, string sender = null, bool writeline = true, string colormessage = null)
            => Log(message, LogType.Info, sender, writeline, colormessage);
        internal void LogWarning(string message, string sender = null, bool writeline = true, string colormessage = null)
            => Log(message, LogType.Warning, sender, writeline, colormessage);
        internal void LogError(string message, string sender = null, bool writeline = true, string colormessage = null)
            => Log(message, LogType.Error, sender, writeline, colormessage);

        internal void Log(string message, LogType type, string sender = null, bool writeline = true, string colormessage = null, bool ytdlpy = false)
        {
            string[] sarr = null;
            if (sender != null) sarr = new string[] { sender };
            Log(message, type, sarr, writeline, colormessage, ytdlpy);
        }

        internal void Log(string message, LogType type, string[] sender = null, bool writeline = true, string colormessage = null, bool ytdlpy = false)
        {
            var args = Logger.Instance.Log(message, type, sender, writeline, colormessage, ytdlpy);
            Log(this, args);
        }
        internal void Log(object sender, LogEventArgs e)
        {
            if (!Options.Quiet)
            {
#if !DEBUG
                if (e.LogType != LogType.Debug)
                {
#endif
                if (!Options.NoColor && Util.TryEnableANSIColor())
                    Console.Write(e.ColoredMessage);
                else
                    Console.Write(e.Message);
#if !DEBUG
                }
#endif
            }

            OnLog?.Invoke(this, e);
        }
        #endregion

        Dictionary<string, int> progressBars = new Dictionary<string, int>();

        internal void ProgressBar(object sender, ProgressEventArgs e, string id, string operation, ConsoleColor color = ConsoleColor.Green) 
        {
            if (Options.Quiet) return;
            int totalChunks = 30;
            //Debug.WriteLine("val: " + e.Value + ", total: " + e.Total);

            lock (ConsoleWriterLock)
            {
                Console.CursorVisible = false;

                int? origTop = null;
                if (progressBars.ContainsKey(id))
                {
                    origTop = Console.CursorTop;
                    Console.CursorTop = progressBars[id];
                }
                else
                {
                    progressBars[id] = Console.CursorTop;
                }

                Console.CursorLeft = 0;
                LogInfo(operation + " " + id + ": ", sender.GetType().Name, false);

                //draw empty progress bar
                Console.CursorLeft += totalChunks;
                Console.Write("|"); //end
                int end = Console.CursorLeft;
                Console.CursorLeft -= totalChunks + 1;

                int numChunksComplete;
                if (e.HasTotal)
                {
                    numChunksComplete = Convert.ToInt16(totalChunks * ((double)e.Value / (double)e.Total));
                }
                else
                {
                    //numChunksComplete = 3 * ((DateTime.Now.Second % 10) + 1);
                    numChunksComplete = 3 * ((DateTime.Now.Millisecond / 100) + 1);
                }

                var backcolor = Console.BackgroundColor;

                if (e.HasTotal)
                {
                    Console.BackgroundColor = color;
                    Console.Write("".PadRight(numChunksComplete));
                }
                else if (numChunksComplete > 0)
                {
                    Console.Write("".PadRight(numChunksComplete-1));
                    Console.BackgroundColor = color;
                    Console.Write("".PadRight(1));
                }

                Console.BackgroundColor = backcolor;
                Console.Write("".PadRight(totalChunks - numChunksComplete));
                Console.CursorLeft = end;

                string s = e.TimePast.ToString(@"hh\:mm\:ss");
                string output = " " + e.SpeedString + " " + s;
                
                Console.Write(output.PadRight(30)); //pad the output so when changing from 3 to 4 digits we avoid text shifting
                Console.Write("\n");
                if (origTop != null)
                {
                    Console.CursorTop = origTop.Value;
                }
                if (e.HasTotal && e.Value == e.Total)
                {
                    Debug.WriteLine("Complete");
                    Console.CursorVisible = true;
                    //progressBars.Remove(id);
                }
            }
        }

        public void SetupHttpClient()
        {
            httpClientHandler = new YTDL_HttpClientHandler
            {
                CookieContainer = new CookieContainer(),
                AllowAutoRedirect = true,
            };
            httpClient = new HttpClient(httpClientHandler, true);
        }

        public void AddExtractors()
        {
            foreach (var type in GetAllExtractorTypes())
            {
                var constructor = type.GetConstructor(new Type[] { typeof(YouTubeDL) });
                if (constructor == null) continue;
                var ie = (IInfoExtractor)constructor.Invoke(new object[] { this });
                ie_instances.Add(ie);
            }
        }

        public void AddExtractor<T>() where T : class, IInfoExtractor
        {
            var type = typeof(T);
            var constructor = type.GetConstructor(new Type[] { typeof(YouTubeDL) });
            if (constructor == null) throw new InvalidOperationException(nameof(T) + " has no valid constructor");
            var ie = (IInfoExtractor)constructor.Invoke(new object[] { this });
            ie_instances.Add(ie);
        }

        public void AddExtractor<T>(Func<T> instance) where T : IInfoExtractor
        {
            ie_instances.Add(instance());
        }

        public void RemoveAllExtractors()
        {
            foreach (var instance in ie_instances)
            {
                if (instance is IDisposable d)
                {
                    d.Dispose();
                }
            }
            ie_instances.Clear();
        }

        public static List<Type> GetAllExtractorTypes()
        {
            Assembly asm = Assembly.GetExecutingAssembly();
            return asm.GetTypes()
                .Where(type =>
                    type.Namespace == "YoutubeDL.Extractors" &&
                    type.Name.EndsWith("IE"))
                .ToList();
        }

        public IInfoExtractor GetInfoExtractorInstance(string name)
        {
            var ie = ie_instances.FirstOrDefault(x => x.Name == name || x.Name == name + "IE");
            if (ie == default) 
            {
                return null;
            }
            return ie;
        }

        public async Task<InfoDict> ExtractInfoAsync(
            string url, bool download = true, string ie_key = null, 
            Dictionary<string,object> extra_info = null, bool process = true, 
            bool force_generic_extractor = false)
        {
            if (ie_key == null && force_generic_extractor)
            {
                ie_key = "Generic";
            }

            List<IInfoExtractor> ies;
            if (ie_key != null)
            {
                ies = new List<IInfoExtractor> { GetInfoExtractorInstance(ie_key) };
            }
            else
            {
                ies = this.ie_instances;
            }

            foreach (IInfoExtractor extractor in ies)
            {
                if (extractor == null || !extractor.Suitable(url)) continue;

                if (!extractor.Working)
                {
                    LogWarning("The program functionality for this site has been marked as broken, and will probably not work.");
                }

                extractor.OnLog += Log;
                extractor.Initialize();

                try
                {
                    InfoDict ie_result = extractor.Extract(url);

                    AddDefaultExtraInfo(ie_result, extractor, url);

                    if (process)
                    {
                        ie_result = await ProcessIEResult(ie_result, download, extra_info);
                    }
                    return ie_result;
                }
                catch (GeoRestrictionException)
                {

                }
                catch (ExtractorException)
                {

                }
                catch (MaxDownloadsReachedException)
                {

                }
                return null;
            }

            return null;

            //LogInfo("native extractors not suitable, switching to python");

            // no native extractor is suitable
            // now we try the python extractors
            /*
            Assembly a = Assembly.Load(new AssemblyName("YoutubeDL.Python"));
            if (a == null) return null;
            var type = a.GetType("YoutubeDL.Python.YoutubeDLPython");
            if (type == null) return null;
            MethodInfo i = type.GetMethod("PythonExtractInfo", BindingFlags.Public | BindingFlags.Static);
            if (i == null) return null;

            try
            {
                return await ((Task<InfoDict>)i.Invoke(null, new object[] { this, url, download, ie_key, extra_info, process, force_generic_extractor })).ConfigureAwait(false);
            }
            catch (InvalidOperationException)
            {
                // python is not installed
                return null;
            }*/
        }

        /// <summary>
        /// Returns the search results of the specified searchExtractor
        /// </summary>
        public Task<InfoDict> GetSearchResults(string search, int numberOfResults = 1, string searchExtractor = "ytsearch", 
            bool download = true, string ie_key = null,
            Dictionary<string, object> extra_info = null, bool process = true)
        {
            return ExtractInfoAsync($"{searchExtractor}{numberOfResults}:{search}", download, null, extra_info, process, false);
        }

        internal static void AddDefaultExtraInfo(InfoDict infoDict, IInfoExtractor ie, string url)
        {
            if (Uri.TryCreate(url, UriKind.Absolute, out Uri result))
            {
                var dict = new Dictionary<string, object>()
                {
                    { "extractor", ie.GetType().Name },
                    { "extractor_key", ie.Name },
                    { "webpage_url", url },
                    { "webpage_url_basename", result.AbsolutePath }
                };
                infoDict.AddExtraInfo(dict, false);
            }
            else
            {
                var dict = new Dictionary<string, object>()
                {
                    { "extractor", ie.GetType().Name },
                    { "extractor_key", ie.Name },
                    { "webpage_url", url },
                    { "webpage_url_basename", url }
                };
                infoDict.AddExtraInfo(dict, false);
            }
        }

        /// <summary>
        /// Take the result of the ie (may be modified) and resolve all unresolved
        /// references (URLs, playlist items).
        /// It will also download the videos if 'download'.
        /// </summary>
        /// <returns>Returns the resolved ie_result.</returns>
        public async Task<InfoDict> ProcessIEResult(InfoDict ie_result, bool download = true, Dictionary<string,object> extra_info = null)
        {
            if (ie_result.GetType() == typeof(ContentUrl) || 
                ie_result.GetType() == typeof(TransparentUrl))
            {
                ContentUrl url = (ContentUrl)ie_result;
                url.Url = Util.SanitizeUrl(url.Url);
                if (Options.ExtractFlat != null)
                {
                    if ((Options.ExtractFlat == "in_playlist" && extra_info != null && extra_info.ContainsKey("playlist")) ||
                        (Options.ExtractFlat.ToUpper() == "TRUE"))
                    {
                        if (Options.ForceUrl && url.Url != null) LogInfo(url.Url);
                        return ie_result;
                    }
                }
            }

            if (ie_result is Video video)
            {
                video.AddExtraInfo(extra_info, false);
                return await ProcessVideoResult(video, download).ConfigureAwait(false);
            }
            else if (ie_result is ContentUrl curl)
            {
                return await ExtractInfoAsync(curl.Url, download, curl.IEKey, extra_info).ConfigureAwait(false);
            }
            else if (ie_result is TransparentUrl turl)
            {
                var info = await ExtractInfoAsync(turl.Url, false, turl.IEKey, extra_info, false).ConfigureAwait(false);
                if (info == null) return info;

                // *force properties*

                //if (info.Type == "url")
                //{
                //    info.Type = "url-transparent";
                //}
                return await ProcessIEResult(info, download, extra_info).ConfigureAwait(false);
            }
            else if (ie_result is Playlist playlist)
            {
                return await ProcessPlaylistResult(playlist, download).ConfigureAwait(false);
            }
            return null;
        }

        public async Task<Video> ProcessVideoResult(Video video, bool download = true)
        {
            //Debug.Assert(video.Type != "video");

            if (video.Id == null)
                throw new ExtractorException("Missing \"id\" field in extractor result");
            if (video.Title == null)
                throw new ExtractorException("Missing \"title\" field in extractor result");

            if (Options.ListThumbnails)
            {
                ListThumbnails(video.Thumbnails);
                return video;
            }

            if (Options.ListSubtitles)
            {
                ListSubtitles(video.Subtitles);
                ListSubtitles(video.AutomaticSubtitles, true);
                return video;
            }

            //List<Subtitle> requestedSubs = ProcessSubtitles(video.Subtitles, video.AutomaticSubtitles); Done in DownloadFormat()

            // *formats*

            // if (video.Formats == null) ;

            // this violates youtube-dl-net syntax and is NOT recommended! This exists just for python compat

            if (Options.ListFormats)
            {
                ListFormats(video.Formats);
                return video;
            }

            if (download)
            {
                await Download(video, Options.Format, true).ConfigureAwait(false);
            }
            return video;
        }

        public async Task ProcessInfo(Video video, IEnumerable<IFormat> formats_to_download, IEnumerable<SubtitleFormat> subtitles_to_download)
        {
            if (Options.MaxDownloads.HasValue &&
                Options.MaxDownloads.Value >= NumDownloads)
            {
                throw new MaxDownloadsReachedException();
            }


            // *_match_entry*
            string reason = null;
            if (reason != null)
            {
                LogInfo("[download] " + reason);
            }

            NumDownloads++;

            string filename = PrepareFilename(video);
            if (filename == null) return;

            ForcedPrintings(video, filename, requestedFormats: formats_to_download);

            if (Options.Simulate) return;

            if (Options.WriteDescription)
                await WriteDescription(video, filename);

            if (Options.WriteAnnotations)
                await WriteAnnotations(video, filename);

            if ((Options.WriteSubtitles || Options.WriteAutomaticSub) && 
                subtitles_to_download != null && subtitles_to_download.Count() >= 1)
            {
                // TODO
            }

            if (Options.WriteInfoJson)
                await WriteInfoJson(video, filename);

            // *write_thumbnails*

            if (Options.SkipDownload) return;

            bool success = true;

            try
            {
                if (formats_to_download != null)
                {
                    // *check if merger is available*

                    // *check if formats compatible*

                    // *check if format already downloaded*

                    List<Task> formatTasks = new List<Task>();

                    foreach (IFormat f in formats_to_download)
                    {
                        formatTasks.Add(DownloadFormat(filename, f));
                    }

                    await Task.WhenAll(formatTasks).ConfigureAwait(false);

                    // **
                }
                else
                {
                    // *get_suitable_downloader*

                    // download
                }
            }
            catch (Exception e)
            {
                throw e;// *catch and manage all exeptions*
            }
        }

        public async Task DownloadFormat(string filename, IFormat format, IProgress<double> progress = null)
        {
            string fname = PrepareFilename(format, filename);
            try
            {
                // download
                if (format is CompFormat cf)
                {
                    var ff = Merger(cf);
                    //if (!ff.Available)
                    if (ff == null)
                    {
                        LogWarning("ffmpeg or avconf is not installed, skipping " + cf.Id);
                        return;
                    }

                    // we dont support merging yet, so just download both
                    string aname = PrepareFilename(cf.AudioFormat, filename);
                    FileDownloader l = FileDownloader.GetSuitableDownloader(cf.AudioFormat.Protocol);
                    l.OnProgress += (sender, e) => {
                        progress?.Report(e.Percent); 
                        ProgressBar(sender, e, cf.AudioFormat.Id, "Downloading");
                    };
                    Task atask = l.DownloadAsync(cf.AudioFormat, aname, true);
                    //downloadTasks.Add(dTask1);
                    cf.AudioFormat.FileName = aname;
                    cf.AudioFormat.IsDownloaded = true;

                    string vname = PrepareFilename(cf.VideoFormat, filename);
                    FileDownloader l2 = FileDownloader.GetSuitableDownloader(cf.VideoFormat.Protocol);
                    l2.OnProgress += (sender, e) => {
                        progress?.Report(e.Percent); 
                        ProgressBar(sender, e, cf.VideoFormat.Id, "Downloading");
                    };
                    Task vtask = l2.DownloadAsync(cf.VideoFormat, vname, true);
                    //downloadTasks.Add(dTask2);
                    cf.VideoFormat.FileName = vname;
                    cf.VideoFormat.IsDownloaded = true;

                    await atask;
                    await vtask;

                    ff.OnLog += Log;
                    ff.OnProgress += (sender, e) => {
                        progress?.Report(e.Percent);
                        ProgressBar(sender, e, cf.Id, "Merging", ConsoleColor.Cyan);
                    };
                    await ff.ProcessAsync(cf, fname);
                    LogDebug("Merged");

                }
                else
                {
                    FileDownloader l3 = FileDownloader.GetSuitableDownloader(format.Protocol);
                    l3.OnProgress += (sender, e) => { 
                        progress?.Report(e.Percent);
                        ProgressBar(sender, e, format.Id, "Downloading");
                    };
                    Task dTask = l3.DownloadAsync(format, fname, true);
                    await dTask;
                    format.FileName = fname;
                    format.IsDownloaded = true;
                }
            }
            catch (Exception e)
            {
                LogError($"Failed to download format {format.Id}: {e.Message}");
                return;
            }

            if (fname != "-")
            {

                // *check and fixup stretched aspect ratio*
                if (format is IVideoFormat vf && vf.StretchedRatio != null && vf.StretchedRatio != 1)
                {
                    if (Options.Fixup == FixupPolicy.Warn)
                    {
                        LogWarning("Stretched Aspect Ratio detected");
                    }
                    else if (Options.Fixup == FixupPolicy.DetectOrWarn)
                    {
                        LogWarning("Stretched Aspect Ratio detected");
                        FFMpegFixupAspectRatioPP pp = new FFMpegFixupAspectRatioPP();
                        pp.OnLog += Log;
                        pp.OnProgress += (sender, e) =>
                        {
                            progress?.Report(e.Percent);
                            ProgressBar(sender, e, vf.Id, "Fixing Aspect Ratio", ConsoleColor.Magenta);
                        };
                        await pp.ProcessAsync(vf, fname);
                    }
                }

                // *manage DASH m4a format*

                // *manage M3U8 format*

                try
                {
                    await PostProcess(fname, format);
                }
                catch (Exception ex)
                {
                    LogError("postprocessing: " + ex.Message);
                }

                // *record_download_archive*
            }

            //LogWarning("ffmpeg or avconf is not installed.");
        }

        public async Task DownloadFormat(Stream outputStream, IFormat format)
        {
            var filename = Path.GetTempFileName();
            await DownloadFormat(filename, format);
            using (var file = File.OpenRead(filename))
            {
                await file.DoubleBufferCopyToAsync(outputStream);
            }
            File.Delete(filename);
        }

        public async Task<InfoDict> ProcessPlaylistResult(Playlist ie_result, bool download = true)
        {
            string playlist = ie_result.Title;
            if (playlist == null)
            {
                playlist = ie_result.Id;
            }

            LogInfo("[download] Downloading playlist: " + playlist);

            int playliststart = Options.PlaylistStart - 1;
            int? playlistend = Options.PlaylistEnd;
            int[] playlistitems = Options.PlaylistItems;

            List<InfoDict> ie_entries = ie_result.Entries;
            List<InfoDict> entries;

            if (playlistitems != null && playlistitems.Length != 0)
            {
                entries = new List<InfoDict>();
                foreach (int itemindex in playlistitems)
                {
                    InfoDict entry = ie_entries[itemindex];
                    entries.Add(entry);
                }
            }
            else if (playlistend != null)
            {
                entries = ie_entries.GetRange(playliststart, (int)playlistend);
            }
            else
            {
                entries = ie_entries.Skip(playliststart).ToList();
            }

            if (Options.PlaylistRandom)
            {
                Util.Shuffle(entries);
            }
            else if (Options.PlaylistReverse)
            {
                entries.Reverse();
            }

            ie_result.Entries = entries;
            List<Task<InfoDict>> playlist_results = new List<Task<InfoDict>>();

            for (int i = 0; i < entries.Count; i++)
            {
                LogInfo("Downloading video " + (i + 1) + " of " + entries.Count, "download");

                // *x_forward_for*

                // *match entry*
                string reason = null;
                if (reason != null)
                {
                    LogInfo(reason, "download");
                    continue;
                }

                var extra = new Dictionary<string, object>() {
                    //{ "n_entries", entries.Count },
                    { "playlist", ie_result },
                    //{ "playlist_id", ie_result["id"] },
                    //{ "playlist_title", ie_result["title"] },
                    //{ "playlist_uploader", ie_result["uploader"] },
                    //{ "playlist_uploader_id", ie_result["uploader_id"] },
                    { "playlist_index", i + playliststart },
                    { "extractor", ie_result.Extractor },
                    { "webpage_url", ie_result.WebpageUrl },
                    { "webpage_url_basename", new Uri(ie_result.WebpageUrl).AbsolutePath },
                    { "extractor_key", ie_result.ExtractorKey }
                };

                var entry_result = ProcessIEResult(entries[i], download, extra);
                playlist_results.Add(entry_result);
            }
            ie_result.Entries = (await Task.WhenAll(playlist_results)).ToList();
            LogInfo("Finished downloading playlist: " + playlist, "download");
            return ie_result;
        }

        public List<SubtitleFormat> ProcessSubtitles(IEnumerable<Subtitle> subs, IEnumerable<Subtitle> autoSubs)
        {
            if (!Options.WriteSubtitles && !Options.WriteAutomaticSub) return null;

            List<Subtitle> availableSubs = new List<Subtitle>();
            if (Options.WriteSubtitles)
            {
                availableSubs.AddRange(subs);
            }
            if (Options.WriteAutomaticSub)
            {
                foreach (var sub in autoSubs)
                {
                    if (!availableSubs.Any(x => x.Name == sub.Name))
                    {
                        availableSubs.Add(sub);
                    }
                }
            }

            if (availableSubs.Count <= 0) return null;

            List<Subtitle> requestedSubs = new List<Subtitle>();
            if (Options.AllSubtitles)
            {
                requestedSubs = availableSubs;
            }
            else if (Options.SubtitlesLangs != null)
            {
                requestedSubs = availableSubs.Where(x => Options.SubtitlesLangs.Contains(x.Name)).ToList();
            }
            else if (availableSubs.Any(x => x.Name == "en"))
            {
                requestedSubs = new List<Subtitle>() { availableSubs.First(x => x.Name == "en") };
            }
            else
            {
                requestedSubs = new List<Subtitle>() { availableSubs.First() };
            }


            string subtitlesformat = Options.SubtitlesFormat;
            string[] pformats = subtitlesformat.Split('/');
            List<SubtitleFormat> sformats = new List<SubtitleFormat>();

            foreach (Subtitle s in requestedSubs)
            {
                if (s.Formats == null)
                {
                    LogWarning($"{s.Name} subtitles not available", "subtitle-parser");
                    continue;
                }

                SubtitleFormat selectedFormat = null;
                foreach (string sf in pformats)
                {
                    if (sf == "best")
                    {
                        selectedFormat = s.Formats.First();
                        break;
                    }

                    selectedFormat = s.Formats.FirstOrDefault(x => x.Extension == sf);
                    if (selectedFormat != default) break;
                }

                if (selectedFormat == null || selectedFormat == default)
                {
                    selectedFormat = s.Formats.First();
                    LogWarning($"No subtitle format found matching \"{subtitlesformat}\" for language {s.Name}, using {selectedFormat.Extension}", "subtitle-parser");
                }

                sformats.Add(selectedFormat);
            }
            return sformats;
        }

        /// <summary>
        /// Its basically a string.Format() at runtime for the properties of the supplied object
        /// </summary>
        /// <returns>Prepared filename</returns>
        private string PrepareFilename(object dict, string prevFilename = null)
        {
            string outtmpl =
                prevFilename ?? Options.OutTemplate;

            Regex r = new Regex("{(?<exp>[^}:]+):?(?<format>[^}]+)?}");

            List<object> ix = new List<object>();
            int i = 0;

            string formatreadystr = r.Replace(outtmpl, match => {
                //var pInfo = video.GetType().GetProperty(match.Groups["exp"].Value, BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
                object value;
                var pInfo = dict.GetType()
                   .GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy)
                   .Where(delegate (PropertyInfo p)
                   {
                       var attr = p.GetCustomAttribute<YTDLMetaAttribute>();
                       return attr?.PythonName == match.Groups["exp"].Value || p.Name == match.Groups["exp"].Value;
                   })
                   .FirstOrDefault();
                if (pInfo == default)
                {
                    var fInfo = dict.GetType()
                        .GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy)
                        .Where(delegate (FieldInfo p)
                        {
                           var attr = p.GetCustomAttribute<YTDLMetaAttribute>();
                            return attr?.PythonName == match.Groups["exp"].Value || p.Name == match.Groups["exp"].Value;
                        })
                        .FirstOrDefault();
                    if (fInfo == default)
                    {
                        return match.Value.Replace("{","{{").Replace("}","}}");
                    }
                    value = fInfo.GetValue(dict);
                }
                else
                    value = pInfo.GetValue(dict);


                ix.Insert(i, value);
                if (match.Groups["format"].Success)
                {
                    return "{" + i++ + ":" + match.Groups["format"].Value + "}";
                }
                else
                {
                    return "{" + i++ + "}";
                }
            });

            string formattedstr = string.Format(formatreadystr, ix.ToArray());

            formattedstr = Path.GetFullPath(formattedstr);

            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(formattedstr));
            }
            catch (Exception ex)
            {
                LogError("Unable to create directory: " + ex.Message);
                return null;
            }

            return formattedstr;
        }

        public List<Func<IFormat, IPostProcessor>> PostProcessors { get; } = new List<Func<IFormat, IPostProcessor>>() { };

        public void AddPostProcessor(Func<IFormat, IPostProcessor> func)
        {
            PostProcessors.Add(func);
        }

        public async Task PostProcess(string filename, IFormat format, CancellationToken token = default)
        {
            foreach (var pFunc in PostProcessors)
            {
                var pp = pFunc(format);
                Type generic = pp.GetType()
                    .GetInterfaces()
                    .Where(
                        x => x.IsGenericType &&
                        x.GetGenericTypeDefinition() == typeof(IPostProcessor<>))
                    .First().GenericTypeArguments[0];
                if (generic.IsAssignableFrom(format.GetType()))
                {
                    await (Task)pp.GetType().GetMethod("ProcessAsync").Invoke(pp, new object[] { format, filename, token });
                }
                else
                {
                    LogWarning($"PostProcesssor is incompatible with format {format.Id} because the format is not a {generic.Name}");
                }
            }
        }

        public IList<IFormat> SelectFormats(Video video, string format)
        {
            return video.Formats.SelectFormats(format, Options.MergeOutputFormat);
        }

        public IList<IFormat> SelectFormats(Video video, string format, string mergeOutputFormat)
        {
            return video.Formats.SelectFormats(format, mergeOutputFormat);
        }

        public async Task Download(Video video, string formatSpec, bool processSubtitles = false)
        {
            var formats_to_download = SelectFormats(video, formatSpec);
            LogInfo($"{video.Id}: downloading video in {formats_to_download.Count} formats", "info");
            if (processSubtitles)
            {
                List<SubtitleFormat> subs = ProcessSubtitles(video.Subtitles, video.AutomaticSubtitles);
                await ProcessInfo(video, formats_to_download, subs);
            }
            else
            {
                await ProcessInfo(video, formats_to_download, null);
            }
        }

        public async Task Download(Video video, string formatSpec, string filename)
        {
            var formats_to_download = SelectFormats(video, formatSpec);
            LogInfo($"{video.Id}: downloading video in {formats_to_download.Count} formats", "info");
            var fn = PrepareFilename(video, filename);

            foreach (var fmt in formats_to_download)
            {
                await DownloadFormat(fn, fmt);
            }
        }

        public void ListThumbnails(IList<Thumbnail> thumbnails)
        {
            // todo
        }

        public void ListSubtitles(IList<Subtitle> subtitles, bool isAutoSubs = false)
        {
            // todo
        }

        public void ListFormats(ICollection<IFormat> formats)
        {
            // todo
        }

        public async Task WriteDescription(Video video, string filename = null, bool changeExtension = true)
        {
            if (string.IsNullOrEmpty(video.Description))
            {
                LogWarning("There is no description to write");
                return;
            }

            if (filename == null)
                filename = PrepareFilename(video);

            if (changeExtension)
                filename = Path.ChangeExtension(filename, "description.txt");

            FileMode fm = Options.NoOverwrites ? FileMode.CreateNew : FileMode.Create;
            try
            {
                using (FileStream f = File.Open(filename, fm, FileAccess.Write))
                {
                    using (StreamWriter w = new StreamWriter(f))
                    {
                        await w.WriteAsync(video.Description);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Cannot write description to file " + filename, ex);
            }
        }

        public async Task WriteAnnotations(Video video, string filename = null, bool changeExtension = true)
        {
            if (string.IsNullOrEmpty(video.Annotations))
            {
                LogWarning("There is no annotation to write");
                return;
            }

            if (filename == null)
                filename = PrepareFilename(video);

            if (changeExtension)
                filename = Path.ChangeExtension(filename, "annotations.xml");

            FileMode fm = Options.NoOverwrites ? FileMode.CreateNew : FileMode.Create;
            try
            {
                using (FileStream f = File.Open(filename, fm, FileAccess.Write))
                {
                    using (StreamWriter w = new StreamWriter(f))
                    {
                        await w.WriteAsync(video.Annotations);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Cannot write annotations to file " + filename, ex);
            }
        }

        public async Task WriteInfoJson(Video video, string filename = null, bool changeExtension = true)
        {
            if (filename == null)
                filename = PrepareFilename(video);

            if (changeExtension)
                filename = Path.ChangeExtension(filename, "json");

            FileMode fm = Options.NoOverwrites ? FileMode.CreateNew : FileMode.Create;
            try
            {
                using (FileStream f = File.Open(filename, fm, FileAccess.Write))
                {
                    using (StreamWriter w = new StreamWriter(f))
                    {
                        string json = JsonSerializer.Serialize(video, video.GetType());
                        await w.WriteAsync(json);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Cannot write info json to file " + filename, ex);
            }
        }

        public async Task WriteThumbnails(ThumbnailCollection thumbs)
        {
            // todo
        }

        private void ForcedPrintings(Video video, string filename, bool incomplete = false, IEnumerable<IFormat> requestedFormats = null)
        {
            if (Options.ForceTitle && (!incomplete || video.Title != null)) 
            {
                LogInfo(video.Title);
            }
            if (Options.ForceID && (!incomplete || video.Id != null))
            {
                LogInfo(video.Id);
            }
            if (Options.ForceUrl && !incomplete)
            {
                if (requestedFormats != null)
                    foreach (IFormat f in requestedFormats) LogInfo(f.Url); // *play_path*
            }
            if (Options.ForceThumbnail && video.Thumbnails != null)
            {
                foreach (Thumbnail t in video.Thumbnails) LogInfo(t.Url);
            }
            if (Options.ForceDescription && video.Description != null)
            {
                LogInfo(video.Description);
            }
            if (Options.ForceFilename && video.Description != null)
            {
                LogInfo(filename);
            }
            if (Options.ForceDuration && video.Duration != null)
            {
                LogInfo(video.Duration.ToString());
            }
            if (Options.ForceFormat && video.Formats != null)
            {
                foreach (IFormat f in video.Formats) LogInfo(f.Url);
            }
            if (Options.ForceJson)
            {
                LogInfo(JsonSerializer.Serialize(video, video.GetType()));
            }
        }

        private bool MatchEntry(Video video, bool incomplete)
        {
            if (Options.MatchTitle != null &&
                !Regex.IsMatch(video.Title, Options.MatchTitle))
            {
                LogInfo($"{video.Title} title did not match pattern + {Options.MatchTitle}");
                return false;
            }
            if (Options.RejectTitle != null &&
                Regex.IsMatch(video.Title, Options.RejectTitle))
            {
                LogInfo($"{video.Title} title matched reject pattern + {Options.MatchTitle}");
                return false;
            }

            if (video.UploadDate < Options.DateRange.Item1 || video.UploadDate > Options.DateRange.Item2)
            {
                return false;
            }
            return true;
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                    if (httpClient != null)
                    {
                        httpClient.Dispose();
                        httpClient = null;
                    }
                    if (httpClientHandler != null)
                    {
                        httpClientHandler.Dispose();
                        httpClientHandler = null;
                    }
                    if (Options != null)
                    {
                        Options = null;
                    }
                    RemoveAllExtractors();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.
                //MethodInfo i = this.GetType().GetMethod("DisposePython", BindingFlags.NonPublic | BindingFlags.Instance);
                //if (i != null) i.Invoke(this, new object[] { disposing });
                

                disposedValue = true;
            }
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
        }
        #endregion
    }
}
