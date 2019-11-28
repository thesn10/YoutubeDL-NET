using Python.Runtime;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace YoutubeDL.Python
{
    static class PyYoutubeDLOptions
    {
        public static Dictionary<string, object> GetPythonSettings(this YoutubeDLOptions opts)
        {
            Dictionary<string, object> pythonSettings = new Dictionary<string, object>();
            foreach (PropertyInfo pInfo in opts.GetType().GetProperties())
            {
                var pythonNameAttr = (YTDLMetaAttribute)pInfo.GetCustomAttribute(typeof(YTDLMetaAttribute));
                if (pythonNameAttr == null) continue;

                string pythonName = pythonNameAttr.PythonName;
                object value = pInfo.GetValue(opts);
                if (value != null)
                {
                    pythonSettings.Add(pythonName, value);
                }
            }
            return pythonSettings;
        }

        public static PyObject ToPyObj(this YoutubeDLOptions opts)
        {
            var d = GetPythonSettings(opts);
            PyDict dict = new PyDict();
            foreach (var kv in d)
            {
                if (!kv.Value.GetType().IsPrimitive) continue;
                dict.SetItem(kv.Key.ToPython(), kv.Value.ToPython());
            }
            return dict;
        }

        public static object GetPyOption(this YoutubeDLOptions opts, string pythonName)
        {
            foreach (PropertyInfo pInfo in opts.GetType().GetProperties())
            {
                var pythonNameAttr = (YTDLMetaAttribute)pInfo.GetCustomAttribute(typeof(YTDLMetaAttribute));
                if (pythonNameAttr == null) continue;
                if (pythonNameAttr.PythonName == pythonName)
                    return pInfo.GetValue(opts);
            }
            return null;
        }
    }
}
