using System;
using System.Collections.Generic;
using System.Text;
using YoutubeDL;
using YoutubeDL.Models;
using YoutubeDL.Python;

namespace YoutubeDL.Models
{
    public static class PyInfoDict
    {
        public static InfoDict FromPythonDict(dynamic pythonInfoDict, string type = null)
        {
            Dictionary<string, object> infoDict = PythonCompat.PythonDictToManaged(pythonInfoDict);
            if (!infoDict.ContainsKey("_type"))
            {
                if (type != null)
                    infoDict.Add("_type", type);
                else
                    infoDict.Add("_type", "video");
            }
            return InfoDict.FromDict(infoDict);
        }
    }
}
