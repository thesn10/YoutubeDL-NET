using System;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using System.Xml;
using System.Linq;
using System.IO;
using System.Text.RegularExpressions;

namespace YoutubeDL.MSBuild
{
    public class DescriptionTask : Task
    {
        public override bool Execute()
        {
            string optionsFile = @"D:\Users\sinan\Documents\Visual Studio 2017\Projects\youtube-dl-net\youtube-dl-net\Options.cs";
            //string xmlFile = @"D:\Users\sinan\Documents\Visual Studio 2017\Projects\youtube-dl-net\youtube-dl-net\YoutubeDL.xml";
            Log.LogMessage(MessageImportance.High, "DESCRIPTIONPARSER");

            Regex r = new Regex(@"<summary>(?<summary>[^<][^\/][^s][^u][^m][^m][^a][^r][^y][^>]+?)<\/summary>\s+\[YTDLMeta\((?<meta>[^)]+)\)]\s+public\s\S+\s(?<name>\S+)");
            Regex dr = new Regex(@"Description\s *=\s *@*""(?<desc>(?>.+?[^\\][^""])+?)""");

            StreamReader sr = new StreamReader(optionsFile);
            string options = sr.ReadToEnd();
            sr.Dispose();

            string replaced = r.Replace(options, (_match) =>
            {
                string meta = _match.Groups["meta"].Value;
                string summary = _match.Groups["summary"].Value
                    .Replace(@"///", "")
                    .Replace("<see cref=\"", "")
                    .Replace("\"/>", "")
                    .Replace("\"", "\\\"")
                    .Trim();

                if (dr.IsMatch(meta))
                {
                    meta = dr.Replace(meta, (dr_match) =>
                    {
                        Group descgroup = dr_match.Groups["desc"];
                        return String.Format("{0}{1}{2}", dr_match.Value.Substring(0, descgroup.Index - dr_match.Index), summary, _match.Value.Substring(descgroup.Index - dr_match.Index + descgroup.Length));
                    });
                }
                else meta += ", Description = @\"" + summary + "\"";

                Group group = _match.Groups["meta"];
                return String.Format("{0}{1}{2}", _match.Value.Substring(0, group.Index - _match.Index), meta, _match.Value.Substring(group.Index - _match.Index + group.Length));
            });

            StreamWriter sw = new StreamWriter(optionsFile);
            sw.BaseStream.Seek(0, SeekOrigin.Begin);
            sw.Write(replaced);
            sw.Flush();
            sw.Dispose();

            return true;
        }
    }
}
