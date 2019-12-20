using System;
using System.Collections.Generic;
using System.Text;

namespace YoutubeDL.Models
{
    public class Video : InfoDict
    {
        [YTDLMeta("id")]
        public string Id { get; set; }
        [YTDLMeta("title")]
        public string Title { get; set; }
        [YTDLMeta("description")]
        public string Description { get; set; }
        [YTDLMeta("annotations")]
        public string Annotations { get; set; }
        [YTDLMeta("duration", AutoFill = false)]
        public TimeSpan Duration { get; set; }

        [YTDLMeta("uploader")]
        public string Uploader { get; set; }
        [YTDLMeta("uploader_id")]
        public string UploaderId { get; set; }
        [YTDLMeta("uploader_url")]
        public string UploaderUrl { get; set; }
        [YTDLMeta("channel")]
        public string Channel { get; set; }
        [YTDLMeta("channel_id")]
        public string ChannelId { get; set; }
        [YTDLMeta("channel_url")]
        public string ChannelUrl { get; set; }

        [YTDLMeta("view_count")]
        public int Views { get; set; }
        [YTDLMeta("like_count")]
        public int Likes { get; set; }
        [YTDLMeta("dislike_count")]
        public int Dislikes { get; set; }
        [YTDLMeta("average_rating")]
        public float AverageRating { get; set; }
        [YTDLMeta("average_rating")]
        public List<string> Categories { get; set; }

        [YTDLMeta("playlist_index")]
        public int PlaylistIndex { get; set; }
        [YTDLMeta("playlist")]
        public Playlist Playlist { get; set; }
        public DateTime UploadDate { get; set; }

        [YTDLMeta("chapter_number")]
        public int? ChapterNr { get; set; }
        [YTDLMeta("season_number")]
        public int? SeasonNr { get; set; }
        [YTDLMeta("episode_number")]
        public int? EpisodeNr { get; set; }

        public FormatCollection Formats { get; set; }
        public ThumbnailCollection Thumbnails { get; set; }
        public SubtitleCollection Subtitles { get; set; }
        public SubtitleCollection AutomaticSubtitles { get; set; }


        public Video() : base()
        {

        }

        public Video(Dictionary<string, object> infoDict) : base(infoDict)
        {
            if (infoDict.TryGetValue("thumbnails", out object thumbnails))
            {
                var thumbnailList = new List<Thumbnail>();
                List<Dictionary<string, object>> xthumbnails = (List<Dictionary<string, object>>)thumbnails;
                foreach (Dictionary<string, object> thumbDict in xthumbnails)
                {
                    thumbDict.Add("_type", "thumbnail");
                    Thumbnail thumbInfoDict = InfoDict.FromDict<Thumbnail>(thumbDict);
                    thumbnailList.Add(thumbInfoDict);
                }
                thumbnailList.Sort();
                for (int i = 0; i < thumbnailList.Count; i++)
                {
                    thumbnailList[i].Url = Util.SanitizeUrl(thumbnailList[i].Url);
                    if (thumbnailList[i].Id == null)
                    {
                        thumbnailList[i].Id = i.ToString();
                    }
                }
                if (AdditionalProperties.ContainsKey("thumbnails")) AdditionalProperties.Remove("thumbnails");
                Thumbnails = new ThumbnailCollection(thumbnailList);
            }
            else if (infoDict.TryGetValue("thumbnail", out object thumbnail))
            {
                var thumbnailList = new List<Thumbnail>();
                Dictionary<string, object> xthumbnail = new Dictionary<string, object>() { { "url", (string)thumbnail } };
                Thumbnail thumbInfoDict = InfoDict.FromDict<Thumbnail>(xthumbnail);
                thumbInfoDict.Url = Util.SanitizeUrl(thumbInfoDict.Url);
                thumbnailList.Add(thumbInfoDict);
                if (AdditionalProperties.ContainsKey("thumbnail")) AdditionalProperties.Remove("thumbnail");
                Thumbnails = new ThumbnailCollection(thumbnailList);
            }

            if (infoDict.TryGetValue("timestamp", out object timestamp))
            {
                UploadDate = DateTimeOffset.FromUnixTimeSeconds(Convert.ToInt64((string)timestamp)).UtcDateTime;
            }

            if (infoDict.TryGetValue("duration", out object duration))
            {
                Duration = TimeSpan.FromSeconds((int)duration);
            }

            if (infoDict.TryGetValue("subtitles", out object subs))
            {
                var subtitles = new List<Subtitle>();
                Dictionary<string, object> xsubs = (Dictionary<string, object>)subs;
                foreach (var kv in xsubs)
                {
                    Subtitle sub = new Subtitle(kv.Key);
                    foreach (Dictionary<string, object> subDict in (List<Dictionary<string, object>>)kv.Value)
                    {
                        subDict.Add("_type", "subtitleformat");
                        SubtitleFormat subf = InfoDict.FromDict<SubtitleFormat>(subDict);
                        subf.Url = Util.SanitizeUrl(subf.Url);
                        // subf.Extension = Util.DetermineExt(subf.Extension);
                        sub.Formats.Add(subf);
                    }
                    subtitles.Add(sub);
                }
                if (AdditionalProperties.ContainsKey("subtitles")) AdditionalProperties.Remove("subtitles");
                Subtitles = new SubtitleCollection(subtitles);
            }

            if (infoDict.TryGetValue("automatic_captions", out object autoSubs))
            {
                var automaticSubtitles = new List<Subtitle>();
                Dictionary<string, object> xsubs = (Dictionary<string, object>)autoSubs;
                foreach (var kv in xsubs)
                {
                    Subtitle sub = new Subtitle(kv.Key);
                    foreach (Dictionary<string, object> subDict in (List<Dictionary<string, object>>)kv.Value)
                    {
                        subDict.Add("_type", "subtitleformat");
                        SubtitleFormat subf = InfoDict.FromDict<SubtitleFormat>(subDict);
                        subf.Url = Util.SanitizeUrl(subf.Url);
                        // subf.Extension = Util.DetermineExt(subf.Extension);
                        sub.Formats.Add(subf);
                    }
                    automaticSubtitles.Add(sub);
                }
                if (AdditionalProperties.ContainsKey("automatic_captions")) AdditionalProperties.Remove("automatic_captions");
                AutomaticSubtitles = new SubtitleCollection(automaticSubtitles);
            }

            if (infoDict.TryGetValue("formats", out object formats))
            {
                var formatlist = new List<IFormat>();
                List<object> xformats = (List<object>)formats;
                foreach (Dictionary<string, object> formatDict in xformats)
                {
                    IFormat fmt = FormatBase.FromDict(formatDict);
                    // fmt.Url = Util.SanitizeUrl(fmt.Url);
                    // fmt.Extension = Util.DetermineExt(fmt.Extension);
                    formatlist.Add(fmt);
                }
                if (AdditionalProperties.ContainsKey("formats")) AdditionalProperties.Remove("formats");
                Formats = new FormatCollection(formatlist);
            }
            else
            {
                // the video is a format
                var formatlist = new List<IFormat>();
                IFormat fmt = FormatBase.FromDict(AdditionalProperties);
                formatlist.Add(fmt);
                Formats = new FormatCollection(formatlist);
            }
        }
    }
}
