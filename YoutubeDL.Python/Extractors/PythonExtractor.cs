using System;
using System.IO;
using System.Threading.Tasks;
using Python.Runtime;
using YoutubeDL.Extractors;
using YoutubeDL.Models;

namespace YoutubeDL.Python.Extractors
{
    public class PythonExtractor : SimpleInfoExtractor, IDisposable
    {
        private static PyScope pythonScope;
        public static PyScope PythonScope
        {
            get
            {
                if (pythonScope == null)
                    pythonScope = Py.CreateScope("extractorscope");
                return pythonScope;
            }
        }

        private static PyObject pythonYoutubeDLModule;
        private bool disposedValue;

        private static PyObject PythonYoutubeDLModule
        {
            get
            {
                if (pythonYoutubeDLModule == null)
                {
                    using (var ms = new MemoryStream(Properties.Resources.fakeytdl))
                    {
                        using (var sr = new StreamReader(ms))
                        {
                            string fakeytdlcode = sr.ReadToEnd();
                            PythonScope.Exec(fakeytdlcode);
                        }
                    }
                    pythonYoutubeDLModule = PythonScope.Get("FakeYTDL");
                }
                return pythonYoutubeDLModule;
            }
        }

        public PythonExtractor(YouTubeDL dl, string name, string module)
        {
            Name = name;
            Module = module;
            ytdl = dl;
        }

        public PythonExtractor(YouTubeDL dl, string name, string matchStr, string module)
        {
            Name = name;
            Module = module;
            ytdl = dl;
            MatchString = matchStr;
        }

        private YouTubeDL ytdl { get; set; }

        public override string Name { get; }

        protected string MatchString { get; }

        public string Module { get; }

        public override string Description => "";

        public override bool Working => true;

        private PyObject extractorInstance { get; set; }

        public override InfoDict Extract(string url)
        {
            var state = GILState(true);
            try
            {
                ytdl.LogDebug("Extracting info from url...");
                dynamic info_dict = (extractorInstance as dynamic).extract(url);
                ytdl.LogDebug("Extracted: " + info_dict.get("title"));
                InfoDict ie_result = PyInfoDict.FromPythonDict(info_dict);
                //state.Dispose();


                return ie_result;
            }
            catch (PythonException p)
            {
                state.Dispose();
                if (p.PythonTypeName == "ExtractorError")
                {
                    ytdl.LogError("Extractor error occurred: " + p.Message);
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

        public override Task<InfoDict> ExtractAsync(string url)
        {
            return Task.FromResult(Extract(url));
        }

        public override bool Suitable(string url)
        {
            GILState(true);
            
                if (MatchString != null)
                {
                    dynamic re = PythonScope.Import("re");
                    return re.match(MatchString, url) != null;
                }

                return (extractorInstance as dynamic).suitable(url);
            
        }

        public string MatchId(string url)
        {
            GILState(true);
            
                if (MatchString != null)
                {
                    dynamic re = PythonScope.Import("re");
                    dynamic match = re.match(MatchString, url);
                    if (match == null) return null;
                    string id1 = (string)match.group("id");
                    if (id1 != null) return id1;
                    string id2 = (string)match.group(2);
                    return id2;
                }

                return (string)(extractorInstance as dynamic)._match_id(url);
            
        }

        private static Py.GILState gilstate;
        public static Py.GILState GILState(bool keep)
        {
            try
            {
                if (!PythonEngine.IsInitialized)
                {
                    PythonEngine.Initialize();
                    PythonEngine.BeginAllowThreads();
                }
                if (keep)
                {
                    if (gilstate == null) gilstate = Py.GIL();
                    return gilstate;
                }
                else return Py.GIL();
            }
            catch (Exception e)
            {
                //ytdl.LogError("Python is not installed! " + e.Message);
                throw new InvalidOperationException("Python is not installed!", e);
            }
        }

        public override void Initialize()
        {
            if (extractorInstance == null)
            {
                GILState(true);
                
                    YTDLPyBridge pyBridge = new YTDLPyBridge(ytdl);
                    var pythonYoutubeDL = (PythonYoutubeDLModule as dynamic)(pyBridge.ToPython());

                    dynamic sys = PythonScope.Import("sys");
                    sys.path.insert(0, AppDomain.CurrentDomain.BaseDirectory);

                    dynamic ext = PythonScope.Import(Module);
                    dynamic ieClass = (ext as PyObject).GetAttr(Name);

                    extractorInstance = ieClass(pythonYoutubeDL);
                
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    extractorInstance.Dispose();
                    extractorInstance = null;
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
