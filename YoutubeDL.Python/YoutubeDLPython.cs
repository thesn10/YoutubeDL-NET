using System;
using System.Threading.Tasks;
using System.Linq;
using System.Net.Http;
using System.IO;
using System.IO.Compression;
using System.Runtime.CompilerServices;
using YoutubeDL.Downloaders;
using YoutubeDL.Python.Extractors;

[assembly: InternalsVisibleTo("YoutubeDL")]
namespace YoutubeDL.Python
{
    public static class YoutubeDLPython
    {
        public static async Task<bool> CheckDownloadYTDLPython(this YouTubeDL ytdl, bool force = false, HttpClient client = null)
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            if (Directory.Exists(baseDir + "/youtube_dl"))
            {
                if (!force) return false;
                Directory.Delete(baseDir + "/youtube_dl", true);
            }

            ytdl.LogInfo("The python version of youtube_dl is required but not installed, downloading and installing youtube-dl...");

            HttpResponseMessage resp;
            if (client == null)
            {
                HttpClientHandler handler = new HttpClientHandler();
                handler.AllowAutoRedirect = false;
                client = new HttpClient(handler);
                resp = await client.GetAsync("https://youtube-dl.org/downloads/latest/");
                client.Dispose();
            }
            else
            {
                resp = await client.GetAsync("https://youtube-dl.org/downloads/latest/");
            }

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

            ytdl.LogInfo($"youtube_dl-{version} installed!");

            return true;
        }

        /*public static void InitPython(this YouTubeDL dl)
        {
            if (!PythonEngine.IsInitialized)
            {
                PythonEngine.Initialize();
                PythonEngine.BeginAllowThreads();
            }
        }

        public static async Task<InfoDict> PythonExtractInfo(
            this YouTubeDL ytdl,
            string url, bool download = true, string ie_key = null,
            Dictionary<string, object> extra_info = null, bool process = true,
            bool force_generic_extractor = false)
        {
            // EXPERIMENTAL CODE
            await CheckDownloadYTDLPython(ytdl);

            Py.GILState state;
            try
            {
                InitPython(ytdl);
                state = Py.GIL();
            } 
            catch (Exception e)
            {
                ytdl.LogError("Python is not installed! " + e.Message);
                throw new InvalidOperationException("Python is not installed!");
            }
            ytdl.LogInfo("Using python lib: " + Runtime.PythonDLL);

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
                        state.Dispose();

                        if (process)
                        {
                            info_dict = await ytdl.ProcessIEResult(ie_result, download, extra_info).ConfigureAwait(false);
                        }

                        return ie_result;
                    }
                    catch (PythonException p)
                    {
                        state.Dispose();
                        if (p.PythonTypeName == "ExtractorError")
                        {
                            ytdl.LogError("Extractor error occurred");
                            throw new ExtractorException(p.Message, p);
                        }
                        else if (p.PythonTypeName == "GeoRestrictionError")
                        {
                            ytdl.LogError("Geo restriction error occurred");
                            throw new GeoRestrictionException(p.Message, p);
                        }
                        else if (p.PythonTypeName == "MaxDownloadsReachedError")
                        {
                            ytdl.LogError("Max downloads reached");
                            throw new MaxDownloadsReachedException(p.Message, p);
                        }
                        else throw p;
                    }
                    catch (Exception e)
                    {
                        state.Dispose();
                        throw e;
                    }
                }
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
                        state.Dispose();

                        if (process)
                        {
                            ie_result = await ytdl.ProcessIEResult(ie_result, download, extra_info).ConfigureAwait(false);
                        }

                        return ie_result;
                    }
                    catch (PythonException p)
                    {
                        state.Dispose();
                        if (p.PythonTypeName == "ExtractorError")
                        {
                            ytdl.LogError("Extractor error occurred");
                            throw new ExtractorException(p.Message, p);
                        }
                        else if (p.PythonTypeName == "GeoRestrictionError")
                        {
                            ytdl.LogError("Geo restriction error occurred");
                            throw new GeoRestrictionException(p.Message, p);
                        }
                        else if (p.PythonTypeName == "MaxDownloadsReachedError")
                        {
                            ytdl.LogError("Max downloads reached");
                            throw new MaxDownloadsReachedException(p.Message, p);
                        }
                        else throw p;
                    }
                    catch (Exception e)
                    {
                        state.Dispose();
                        throw e;
                    }
                }

                state.Dispose();
                return null;
            }
        }

        public static async Task<string> PyGetId(this YouTubeDL ytdl, string url)
        {
            // EXPERIMENTAL CODE
            await CheckDownloadYTDLPython(ytdl);

            Py.GILState state;
            try
            {
                InitPython(ytdl);
                state = Py.GIL();
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
                    string id = (string)match.group(2);
                    state.Dispose();
                    return id;
                }
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
                    state.Dispose();
                    return (string)ie._match_id(url);
                }
                state.Dispose();
                return null;
            }
        }*/

        public static void AddPythonExtractors(this YouTubeDL ytdl)
        {
            LazyExtractors.LoadLazyExtractors();
            foreach (var r in LazyExtractors.Extractors)
            {
                var pyIe = new PythonExtractor(ytdl, r.Key, r.Value.Item1, r.Value.Item2);
                ytdl.ie_instances.Add(pyIe);
            }
        }

        /*
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
        }*/
    }
}
