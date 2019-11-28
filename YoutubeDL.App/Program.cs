using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using YoutubeDL;

namespace YoutubeDL.App
{
    class Program
    {
        static void Main(string[] args)
        {
            switch (args[0])
            {
                case "dl":
                case "download":
                    Download(args);
                    break;
                case "show":
                case "list":
                    List(args);
                    break;
                case "update":
                    Update(args);
                    break;
                case "man":
                case "doc":
                case "help":
                    Help(args);
                    break;
                default:
                    break;
            }
        }

        public static void Download(params string[] args)
        {
            var options = PopulateOptions(out string url, args.Skip(1).ToArray());
            YouTubeDL dl = new YouTubeDL(options);
            dl.ExtractInfoAsync(url).GetAwaiter().GetResult();
        }

        public static void List(params string[] args)
        {
            var options = PopulateOptions(out string url, args.Skip(2).ToArray());
            switch (args[1])
            {
                case "formats":
                    options.ListFormats = true;
                    break;
                case "thumbnails":
                    options.ListThumbnails = true;
                    break;
                case "subtitles":
                    options.ListSubtitles = true;
                    break;
            }
            YouTubeDL dl = new YouTubeDL(options);
            dl.ExtractInfoAsync(url).GetAwaiter().GetResult();
        }

        public static void Update(params string[] args)
        {

        }

        public static void Help(params string[] args)
        {
            string desc = YoutubeDLOptions.GetPropDescription(args[1]);
            Console.WriteLine(args[1] + ":\n" + desc);
        }

        public static YoutubeDLOptions PopulateOptions(out string url, params string[] args)
        {
            YoutubeDLOptions options = new YoutubeDLOptions();
            Type tOpts = typeof(YoutubeDLOptions);
            url = null;

            for (int i = 0; i < args.Length;)
            {
                if (args[i].StartsWith("--"))
                {
                    string pname = args[i].Substring(2).Replace('-', '_');
                    var prop = tOpts.GetProperties().Where(x => 
                    {
                        var attr = x.GetCustomAttribute<YTDLMetaAttribute>();
                        return attr.PythonName == pname || attr.ArgName == pname || x.Name == pname; 
                    }).FirstOrDefault();
                    if (prop == default)
                    {
                        Console.WriteLine($"Failed: Argument does not exist: {args[i]}");
                    }
                    string value = args[i + 1];

                    var pType = prop.PropertyType;
                    if (pType == typeof(string))
                    {
                        prop.SetValue(options, value);
                    }
                    else if (pType == typeof(int))
                    {
                        if (int.TryParse(value, out int ival))
                        {
                            prop.SetValue(options, ival);
                        }
                        else
                        {
                            Console.WriteLine($"Failed to parse int value of argument {args[i]}");
                        }
                    }
                    else if (pType == typeof(bool))
                    {
                        if (bool.TryParse(value, out bool ival))
                        {
                            prop.SetValue(options, ival);
                        }
                        else
                        {
                            Console.WriteLine($"Failed to parse bool value of argument {args[i]}");
                        }
                    }
                    else if (pType == typeof(int[]))
                    {
                        string[] values = value.Split(",");
                        List<int> ints = new List<int>();
                        foreach (string val in values)
                        {
                            if (int.TryParse(value, out int ival))
                            {
                                ints.Add(ival);
                            }
                            else
                            {
                                Console.WriteLine($"Failed to parse int array value of argument {args[i]}");
                                break;
                            }
                        }
                    }
                    i += 2;
                }
                else if (args[i].StartsWith("-"))
                {
                    i++;
                }
                else
                {
                    //Console.WriteLine("GOT URL: " + args[i]);
                    url = args[i];
                    i++;
                }
            }
            return options;
        }
    }
}
