using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Python.Runtime;

namespace YoutubeDL.Python
{
    public class UnitTestYoutubeDL
    {
        public YoutubeDLOptions Options = new YoutubeDLOptions();
        public PyScope PyScope { get; set; }
        public void ToScreen(string message, bool skip_eol = false)
        {
            //Log(message, LogType.Info, writeline: !skip_eol, ytdlpy: true);
        }

        public void ReportError(string message)
        {
            Console.WriteLine("\u001b[91mERROR: " + message + "\u001b[0m");
            //Log(message, LogType.Error, ytdlpy: true);
        }

        public void ReportWarning(string message)
        {
            Console.WriteLine("\u001b[93mWARNING: " + message + "\u001b[0m");
            //Log(message, LogType.Warning, ytdlpy: true);
        }



        public void Failed(string id, string name, string message)
        {
            Console.WriteLine("\u001b[91m" + name + "/" + id + " - FAILED: " + message + "\u001b[0m");
        }

        public void Success(string id, string name)
        {
            Console.WriteLine("\u001b[92m" + name + "/" + id + " - SUCCESS\u001b[0m");
        }

        public void TestPythonExtractors()
        {
            using (Py.GIL())
            {
                PyScope = Py.CreateScope();
                

                dynamic os = PyScope.Import("os");
                os.chdir(Environment.CurrentDirectory);

                dynamic ytdl_extactor = PyScope.Import("fake_youtube_dl.youtube_dl.extractor");
                //LogInfo("imported youtube_dl");

                //PyScope.Exec("from youtube_dl.extractor import get_info_extractor, gen_extractor_classes");
                //PyScope.Exec("from youtube_dl import YoutubeDL");
                dynamic extractors = ytdl_extactor.gen_extractor_classes();

                dynamic fakeytdl = PyScope.Import("fake_youtube_dl");
                dynamic fakeytdlclass = fakeytdl.FakeYTDL(this.ToPython());

                foreach (dynamic ie in extractors)
                {
                    dynamic extractor = ytdl_extactor.get_info_extractor(ie.ie_key())(fakeytdlclass);
                    foreach (var t in extractor.get_testcases(true))
                    {
                        string url = (string)t.get("url", "");
                        dynamic inf = t.get("info_dict");
                        string id = "unknown";
                        if (inf != null)
                        {
                            string title = (string)inf.get("title", "unknown");
                            id = (string)inf.get("id", title);
                        }

                        try
                        {
                            dynamic infoDict = extractor.extract(url);
                            Success(id, (string)ie.ie_key());
                        }
                        catch (Exception ex)
                        {
                            Failed(id, (string)ie.ie_key(), ex.Message);
                        }
                    }

                    //PyScope.Exec("from .fakeytdl import FakeYTDL");
                    //dynamic fakeytdl = PyScope.Eval("FakeYTDL");
                    //dynamic fakeytdlclass = fakeytdl(this.ToPython());
                    //PyScope.Exec("from fake_youtube_dl.youtube_dl.extractor.youtube import *");

                }

            }
        }
    }
}
