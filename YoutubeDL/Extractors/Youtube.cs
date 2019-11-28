using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using YoutubeDL.Models;

namespace YoutubeDL.Extractors
{
    public class YoutubeIE : MultiInfoExtractor
    {
        protected const string _VALID_URL = "^hello";
        public YoutubeIE(IManagingDL dl) : base(dl)
        {

        }

        public override bool Working => false;

        public override Regex MatchRegex => new Regex(_VALID_URL);

        public override string Description => "meh";

        public override string Name => "youtube";

        public override void Initialize()
        {
            throw new NotImplementedException();
        }

        [ExtractionFunc(_VALID_URL, true)]
        protected Video GetVideo(string url)
        {
            return null;// new InfoDict("video") { { "hallo", "welt" } };
        }
    }
}
