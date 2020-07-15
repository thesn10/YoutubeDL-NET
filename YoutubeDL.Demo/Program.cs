using System;
using System.Threading.Tasks;
using YoutubeDL;
using YoutubeDL.Models;
using YoutubeDL.Python;

namespace youtube_dl_net_demo
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("[YOUTUBE DL FOR .NET]");
            //YoutubeDL ytdl = new YoutubeDL();
            Test().GetAwaiter().GetResult();
            //UnitTest();

            Console.ReadKey();
        }

        public static void UnitTest()
        {
            YoutubeDL.Python.UnitTestYoutubeDL ytdl = new YoutubeDL.Python.UnitTestYoutubeDL();
            ytdl.TestPythonExtractors();
        }

        public static async Task Test()
        {
            YouTubeDL ytdl = new YouTubeDL();
            await ytdl.CheckDownloadYTDLPython(false);
            ytdl.AddPythonExtractors();
            ytdl.Options.Format = "bestvideo+m4a/best";
            //ytdl.Options.ExtractFlat = "in_playlist";
            //ytdl.Options.MergeOutputFormat = "mp4";
            //ytdl.Options.Format = "bestaudio";
            //ytdl.Options.Verbose = true;
            //InfoDict dict = await ytdl.GetSearchResults("far out - overdrive", 10);
            //InfoDict dict = await ytdl.ExtractInfoAsync("https://www.youtube.com/playlist?list=PL8SwD_foum9yWCuNn1IyqkZI7EMmnzxcr", download: false);
            InfoDict dict = await ytdl.ExtractInfoAsync("https://www.youtube.com/watch?v=X1jMMFOqxEw"); //https://www.youtube.com/watch?v=oP8TAcUc17w");

            if (dict is Video video)
            {
                Console.WriteLine("YoutubeDL for .NET Extracted Video " + video.Id + ": " + video.Title);
            }
            if (dict is Playlist playlist)
            {
                Console.WriteLine("YoutubeDL for .NET Extracted Playlist " + playlist.Id + ": " + playlist.Title);
                foreach (ContentUrl d in playlist.Entries)
                {
                    //Console.WriteLine(d.GetType().Name + ":");
                    foreach (var prop in d.AdditionalProperties)
                    {
                        //Console.WriteLine(prop.Key + " = " + prop.Value);
                    }
                }

                //await ytdl.ProcessIEResult(playlist.Entries[0], true);
            }

            //InfoDict dict2 = ytdl.ExtractInfo("https://www.youtube.com/watch?v=X1jMMFOqxEw");
        }
    }
}
