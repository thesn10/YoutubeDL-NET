using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Reflection;
using System.Reflection.Emit;
using Python.Runtime;
using System.Net;
using System.Threading.Tasks;
using System.Linq;
using System.Net.Http;
using YoutubeDL.Models;
//using System.Text.Json;
using System.Resources;
using System.IO;
using System.IO.Compression;
using YoutubeDL;
using YoutubeDL.Python;
using System.Runtime.CompilerServices;
using YoutubeDL.Downloaders;

[assembly: InternalsVisibleTo("YoutubeDL")]
namespace YoutubeDL.Python
{
    internal static class YoutubeDLPython
    {
        private static PyScope PyScope;
        public static async Task CheckDownloadYTDLPython(YouTubeDL ytdl)
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            if (Directory.Exists(baseDir + "/youtube_dl")) return;

            ytdl.LogInfo("The python version of youtube_dl is required but not installed, downloading and installing youtube-dl...");

            HttpClientHandler handler = new HttpClientHandler();
            handler.AllowAutoRedirect = false;

            using HttpClient client = new HttpClient(handler);

            var resp = await client.GetAsync("https://youtube-dl.org/downloads/latest/");
            string version = resp.Headers.Location.Segments.Last().Replace("/", "").Trim();

            string repoUrl = $"https://github.com/ytdl-org/youtube-dl/archive/{version}.zip";
            HttpFD httpfd = new HttpFD();
            httpfd.OnProgress += (sender, e) => ytdl.ProgressBar(sender, e, "youtube_dl-" + version, "Downloading");
            await httpfd.SingleThreadedDownload(repoUrl, baseDir + "/youtube_dl.zip");
            ytdl.LogInfo($"Extracting youtube_dl-{version}");

            ZipFile.ExtractToDirectory(baseDir + "/youtube_dl.zip", baseDir);
            File.Delete(baseDir + "/youtube_dl.zip");
            Directory.Move(baseDir + $"/youtube-dl-{version}/youtube_dl", baseDir + "/youtube_dl");
            Directory.Delete(baseDir + $"/youtube-dl-{version}", true);
        }

        public static async Task<InfoDict> PythonExtractInfo(
            this YouTubeDL ytdl,
            string url, bool download = true, string ie_key = null,
            Dictionary<string, object> extra_info = null, bool process = true,
            bool force_generic_extractor = false)
        {
            // EXPERIMENTAL CODE
            await CheckDownloadYTDLPython(ytdl);

            IntPtr state2 = IntPtr.Zero;
            bool mainRun = false;
            Py.GILState state;
            try
            {
                if (!PythonEngine.IsInitialized)
                {
                    mainRun = true;
                    PythonEngine.Initialize();
                    PythonEngine.BeginAllowThreads();
                    state2 = PythonEngine.AcquireLock();
                    state = null;
                }
                else state = Py.GIL();
            } 
            catch 
            {
                ytdl.LogError("Python is not installed!");
                throw new InvalidOperationException("Python is not installed!");
            }

            if (PyScope == null)
                PyScope = Py.CreateScope("extractorscope");

            if (ytdl.Options.LazyLoad)
            {
                dynamic re = PyScope.Import("re");
                dynamic sys = PyScope.Import("sys");
                sys.path.insert(0, AppDomain.CurrentDomain.BaseDirectory);
                ytdl.LogDebug("Loading python extractors");
                LazyExtractors.LoadLazyExtractors();
                foreach (var r in LazyExtractors.Extractors)
                {
                    dynamic match = re.match(r.Value.Item1, url);
                    if (match == null) continue;

                    ytdl.LogDebug("Match found: " + r.Value.Item2);

                    ytdl.LogDebug("Injecting youtube-dl python to .NET bridge");
                    using (var ms = new MemoryStream(Properties.Resources.fakeytdl))
                    {
                        using (var sr = new StreamReader(ms))
                        {
                            string fakeytdlcode = sr.ReadToEnd();
                            PyScope.Exec(fakeytdlcode);
                        }
                    }

                    YTDLPyBridge pyBridge = new YTDLPyBridge(ytdl);

                    dynamic fakeytdlmod = PyScope.Get("FakeYTDL");
                    dynamic fakeytdl = fakeytdlmod(pyBridge.ToPython());

                    ytdl.LogDebug("Importing extractor (slow)");
                    dynamic ext = PyScope.Import(r.Value.Item2);
                    dynamic ieClass = (ext as PyObject).GetAttr(r.Key);

                    dynamic ie = ieClass(fakeytdl);

                    try
                    {
                        ytdl.LogDebug("Extracting...");
                        dynamic info_dict = ie.extract(url);
                        ytdl.LogDebug("Extracted: " + info_dict.get("title"));
                        InfoDict ie_result = PyInfoDict.FromPythonDict(info_dict);

                        AddDefaultExtraInfo(ytdl, ie_result, ie, url);
                        if (mainRun)
                            PythonEngine.ReleaseLock(state2);
                        else
                            state.Dispose();

                        if (process)
                        {
                            info_dict = await ytdl.ProcessIEResult(ie_result, download, extra_info).ConfigureAwait(false);
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
                    if (mainRun)
                        PythonEngine.ReleaseLock(state2);
                    else
                        state.Dispose();
                    return null;
                }
                if (mainRun)
                    PythonEngine.ReleaseLock(state2);
                else
                    state.Dispose();
                return null;
            }
            else
            {

                dynamic os = PyScope.Import("os");
                os.chdir(AppDomain.CurrentDomain.BaseDirectory);

                ytdl.LogDebug("Importing extractors (slow)");
                dynamic ytdl_extactor = PyScope.Import("youtube_dl.extractor");

                dynamic extractors = ytdl_extactor.gen_extractor_classes();
                foreach (dynamic ie in extractors)
                {
                    if (!ie.suitable(url)) continue;

                    ytdl.LogDebug("Injecting youtube-dl python to .NET bridge");
                    using (var ms = new MemoryStream(Properties.Resources.fakeytdl))
                    {
                        using (var sr = new StreamReader(ms))
                        {
                            string fakeytdlcode = sr.ReadToEnd();
                            PyScope.Exec(fakeytdlcode);
                        }
                    }

                    YTDLPyBridge pyBridge = new YTDLPyBridge(ytdl);

                    dynamic fakeytdlmod = PyScope.Get("FakeYTDL");
                    dynamic fakeytdlclass = fakeytdlmod(pyBridge.ToPython());

                    dynamic extractor = ytdl_extactor.get_info_extractor(ie.ie_key())(fakeytdlclass);
                    try
                    {
                        dynamic info_dict = extractor.extract(url);
                        ytdl.Log("Extracted: " + info_dict.get("title"), LogType.Debug);
                        InfoDict ie_result = PyInfoDict.FromPythonDict(info_dict);

                        AddDefaultExtraInfo(ytdl, ie_result, extractor, url);
                        if (mainRun)
                            PythonEngine.ReleaseLock(state2);
                        else
                            state.Dispose();

                        if (process)
                        {
                            ie_result = await ytdl.ProcessIEResult(ie_result, download, extra_info).ConfigureAwait(false);
                        }

                        return ie_result;
                    }
                    catch (GeoRestrictionException e)
                    {
                        ytdl.LogError("Geo restriction error occurred");
                        throw e;
                    }
                    catch (ExtractorException e)
                    {
                        ytdl.LogError("Extractor error occurred");
                        throw e;
                    }
                    catch (MaxDownloadsReachedException e)
                    {
                        ytdl.LogError("Max downloads reached");
                        throw e;
                    }
                    if (mainRun)
                        PythonEngine.ReleaseLock(state2);
                    else
                        state.Dispose();
                    return null;
                }

                if (mainRun)
                    PythonEngine.ReleaseLock(state2);
                else
                    state.Dispose();
                return null;
            }
        }

        public static void AddDefaultExtraInfo(this YouTubeDL ytdl, InfoDict infoDict, dynamic python_ie, string url)
        {
            string abspath = url;
            try { abspath = new Uri(url).AbsolutePath; }
            catch { };

            var dict = new Dictionary<string, object>()
            {
                { "extractor", (string)python_ie.__class__.__name__ },
                { "extractor_key", (string)python_ie.ie_key() },
                { "webpage_url", url },
                { "webpage_url_basename", abspath }
            };
            infoDict.AddExtraInfo(dict, false);
        }
    }
}
