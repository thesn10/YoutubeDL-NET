using System;
using System.Threading.Tasks;
using YoutubeDL;
using YoutubeDL.Models;

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
            //ytdl.Options.Verbose = true;
            //ytdl.SetupLazyLoad();
            //InfoDict dict = ytdl.ExtractInfo("https://www.youtube.com/playlist?list=PL8SwD_foum9yWCuNn1IyqkZI7EMmnzxcr", download: false);
            InfoDict dict = await ytdl.ExtractInfoAsync("https://www.youtube.com/watch?v=L6ZSUyI9tx4").ConfigureAwait(false);//"https://www.youtube.com/watch?v=u2id3z1vw8c")//"https://www.youtube.com/watch?v=OKnLb4j2o9s");//"https://www.youtube.com/watch?v=g5AsOiLaS4w");

            if (dict is Video video)
            {
                Console.WriteLine("YoutubeDL for .NET Extracted Video " + video.Id + ": " + video.Title);
            }
            if (dict is Playlist playlist)
            {
                Console.WriteLine("YoutubeDL for .NET Extracted Playlist " + playlist.Id + ": " + playlist.Title);
            }

            //InfoDict dict2 = ytdl.ExtractInfo("https://www.youtube.com/watch?v=g5AsOiLaS4w", process: false);
        }
    }
}
