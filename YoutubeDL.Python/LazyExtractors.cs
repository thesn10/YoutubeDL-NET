using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.IO;
using Python.Runtime;

namespace YoutubeDL
{
    static class LazyExtractors
    {
        public static Dictionary<string, (string, string)> Extractors { get; private set; } = null;
        public static string LazyLoadFilePath { get; } = Environment.CurrentDirectory + @"\youtube_dl\extractor\lazy_extractors.bin";

        public static void LoadLazyExtractors(bool forceRebuild = false, bool forceReload = false)
        {
            if (!File.Exists(LazyLoadFilePath) || forceRebuild)
            {
                BuildLazyExtractors();
                return;
            }
            else if (Extractors != null && !forceReload) return;

            Extractors = new Dictionary<string, (string, string)>();
            using (FileStream stream = File.OpenRead(LazyLoadFilePath))
            {
                BinaryFormatter formatter = new BinaryFormatter();
                Extractors = (Dictionary<string, (string, string)>)formatter.Deserialize(stream);
            }
        }

        private static void BuildLazyExtractors()
        {
            Extractors = new Dictionary<string, (string, string)>();
            using (Py.GIL())
            {
                using (PyScope ps = Py.CreateScope())
                {
                    dynamic ext = ps.Import("youtube_dl.extractor");
                    foreach (dynamic eclass in ext._ALL_CLASSES)
                    {
                        string name = (string)eclass.__name__;
                        string module = (string)eclass.__module__;
                        string validurl = (eclass as PyObject).GetAttr("_VALID_URL", null)?.ToString();
                        if ((eclass as PyObject).HasAttr("_make_valid_url"))
                        {
                            validurl = eclass._make_valid_url();
                        }

                        Extractors.Add(name, (validurl, module));
                    }
                }
            }

            using (FileStream stream = File.OpenWrite(LazyLoadFilePath)) 
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(stream, Extractors);
            }
        }
    }
}
