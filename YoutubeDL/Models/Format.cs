using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using YoutubeDL.Downloaders;

namespace YoutubeDL.Models
{
    public interface IFormat : IDownloadable
    {
        string Name { get; set; }
        string Id { get; set; }
        string Extension { get; set; }
        string Note { get; set; }
        string Quality { get; set; }
        string Protocol { get; set; }
    }

    public interface IAudioFormat : IFormat
    {
        string AudioCodec { get; set; }
        int AudioBitrate { get; set; }
        int AudioSampleRate { get; set; }
    }

    public interface IVideoFormat : IFormat
    {
        string VideoCodec { get; set; }
        int Width { get; set; }
        int Height { get; set; }
        double FPS { get; set; }
        int VideoBitrate { get; set; }
        int? StretchedRatio { get; set; }
    }

    public interface IMuxedFormat : IAudioFormat, IVideoFormat
    {
        float TotalBitrate { get; set; }
    }

    public abstract class FormatBase : InfoDict, IFormat
    {
        [YTDLMeta("format")]
        public string Name { get; set; }
        [YTDLMeta("format_id")]
        public string Id { get; set; }
        [YTDLMeta("url")]
        public string Url { get; set; }
        [YTDLMeta("ext")]
        public string Extension { get; set; }
        [YTDLMeta("format_note")]
        public string Note { get; set; }
        [YTDLMeta("quailty")]
        public string Quality { get; set; }

        public Dictionary<string, string> HttpHeaders { get; set; }
        [YTDLMeta("downloader_options")]
        public Dictionary<string, object> DownloaderOptions { get; set; }
        [YTDLMeta("protocol")]
        private string protocol;
        public string Protocol
        {
            get
            {
                if (protocol == null)
                    protocol = Util.DetermineProtocol(Url);
                return protocol;
            }
            set => protocol = value;
        }
        public bool IsDownloaded { get; set; }
        public string FileName { get; set; }
        public FormatBase() : base() { }

        public FormatBase(Dictionary<string, object> infoDict) : base(infoDict) { }

        public static FormatBase FromDict(Dictionary<string, object> infoDict)
        {
            string sacodec = (string)infoDict.GetValueOrDefault("acodec");
            string svcodec = (string)infoDict.GetValueOrDefault("vcodec");
            bool acodec = sacodec != default && sacodec != "none";
            bool vcodec = svcodec != default && svcodec != "none";

            if (acodec && vcodec) 
                return new MuxedFormat(infoDict);
            else if (acodec) 
                return new AudioFormat(infoDict);
            else if (vcodec)
                return new VideoFormat(infoDict);
            else
                return new VideoFormat(infoDict);
        }
    }

    public class AudioFormat : FormatBase, IAudioFormat
    {
        [YTDLMeta("acodec")]
        public string AudioCodec { get; set; }
        [YTDLMeta("abr")]
        public int AudioBitrate { get; set; }
        [YTDLMeta("asr")]
        public int AudioSampleRate { get; set; }
        public AudioFormat() : base() { }

        public AudioFormat(Dictionary<string, object> infoDict) : base(infoDict) { }
    }

    public class VideoFormat : FormatBase, IVideoFormat
    {
        [YTDLMeta("vcodec")]
        public string VideoCodec { get; set; }
        [YTDLMeta("width")]
        public int Width { get; set; }
        [YTDLMeta("height")]
        public int Height { get; set; }
        [YTDLMeta("fps")]
        public double FPS { get; set; }
        [YTDLMeta("vbr")]
        public int VideoBitrate { get; set; }
        [YTDLMeta("stretched_ratio")]
        public int? StretchedRatio { get; set; }
        public VideoFormat() : base() { }

        public VideoFormat(Dictionary<string, object> infoDict) : base(infoDict) { }
    }

    public class MuxedFormat : FormatBase, IMuxedFormat
    {
        [YTDLMeta("acodec")]
        public string AudioCodec { get; set; }
        [YTDLMeta("abr")]
        public int AudioBitrate { get; set; }
        [YTDLMeta("asr")]
        public int AudioSampleRate { get; set; }

        [YTDLMeta("vcodec")]
        public string VideoCodec { get; set; }
        [YTDLMeta("width")]
        public int Width { get; set; }
        [YTDLMeta("height")]
        public int Height { get; set; }
        [YTDLMeta("fps")]
        public double FPS { get; set; }
        [YTDLMeta("vbr")]
        public int VideoBitrate { get; set; }
        [YTDLMeta("stretched_ratio")]
        public int? StretchedRatio { get; set; }

        [YTDLMeta("tbr")]
        public float TotalBitrate { get; set; }
        public MuxedFormat() : base() { }

        public MuxedFormat(Dictionary<string, object> infoDict) : base(infoDict) { }
    }

    public class CompFormat : MuxedFormat
    {
        public IAudioFormat AudioFormat { get; }
        public IVideoFormat VideoFormat { get; }
        public CompFormat(IAudioFormat audio, IVideoFormat video) : base()
        {
            AudioFormat = audio;
            VideoFormat = video;
            Id = video.Id + "+" + audio.Id;
            Name = video.Name + "+" + audio.Name;
            Width = video.Width;
            Height = video.Height;
            FPS = video.FPS;
            VideoCodec = video.VideoCodec;
            VideoBitrate = video.VideoBitrate;
            StretchedRatio = video.StretchedRatio;
            AudioCodec = audio.AudioCodec;
            AudioBitrate = audio.AudioBitrate;
            Extension = video.Extension;
        }
    }
    /*
    public class Format : InfoDict, IDownloadable, IFormat
    {
        [YTDLMeta("format")]
        public string Name { get; set; }
        [YTDLMeta("format_id")]
        public string Id { get; set; }
        [YTDLMeta("url")]
        public string Url { get; set; }
        [YTDLMeta("ext")]
        public string Extension { get; set; }
        [YTDLMeta("format_note")]
        public string Note { get; set; }
        [YTDLMeta("quailty")]
        public string Quality { get; set; }

        public Dictionary<string, string> HttpHeaders { get; set; }
        [YTDLMeta("downloader_options")]
        public Dictionary<string, object> DownloaderOptions { get; set; }

        [YTDLMeta("vcodec")]
        public string VideoCodec { get; set; }
        [YTDLMeta("acodec")]
        public string AudioCodec { get; set; }
        [YTDLMeta("width")]
        public int Width { get; set; }
        [YTDLMeta("height")]
        public int Height { get; set; }
        [YTDLMeta("fps")]
        public double FPS { get; set; }
        [YTDLMeta("vbr")]
        public int VideoBitrate { get; set; }
        [YTDLMeta("abr")]
        public int AudioBitrate { get; set; }
        [YTDLMeta("tbr")]
        public float TotalBitrate { get; set; }
        [YTDLMeta("asr")]
        public int AudioSampleRate { get; set; }
        [YTDLMeta("stretched_ratio")]
        public string StretchedRatio { get; set; }

        [YTDLMeta("protocol")]
        private string protocol;
        public string Protocol
        {
            get
            {
                if (protocol == null)
                    protocol = Util.DetermineProtocol(Url);
                return protocol;
            }
            set => protocol = value;
        }

        public bool IsDownloaded { get; set; }

        public string FileName { get; set; }

        public Format() : base() { }

        public Format(Dictionary<string, object> infoDict) : base(infoDict) { }

        public async Task ConvertAsync()
        {
            if (!IsDownloaded) throw new InvalidOperationException("Cannot convert a format that has not been downloaded");
        }

        /// <summary>
        /// Perform a basic download operation independent of a <see cref="YoutubeDL"/> instance options. 
        /// For more (like progressbar, format selector, etc.), use <see cref="YoutubeDL.DownloadFormat"/>
        /// </summary>
        /// <param name="filename">Filename to download to</param>
        /// <returns></returns>
        public async Task DownloadAsync(string filename)
        {
            FileDownloader dl = FileDownloader.GetSuitableDownloader(Protocol);
            await dl.DownloadAsync(this, filename);
        }

        /// <summary>
        /// Perform a basic download operation independent of a <see cref="YoutubeDL"/> instance and its options. 
        /// For more (like progressbar, format selector, etc.), use <see cref="YoutubeDL.DownloadFormat"/>
        /// </summary>
        /// <param name="filename">Filename to download to</param>
        /// <returns></returns>
        public void Download(string filename)
        {
            FileDownloader dl = FileDownloader.GetSuitableDownloader(Protocol);
            dl.Download(this, filename);
        }
    }*/
    /*
    public class CompositeFormat : Format
    {
        public Format AudioFormat { get; }
        public Format VideoFormat { get; }
        new public string Protocol { get; }
        public CompositeFormat(Format audio, Format video) : base()
        {
            AudioFormat = audio;
            VideoFormat = video;
            Id = video.Id + "+" + audio.Id;
            Name = video.Name + "+" + audio.Name;
            Width = video.Width;
            Height = video.Height;
            FPS = video.FPS;
            VideoCodec = video.VideoCodec;
            VideoBitrate = video.VideoBitrate;
            StretchedRatio = video.StretchedRatio;
            AudioCodec = audio.AudioCodec;
            AudioBitrate = audio.AudioBitrate;
            Extension = video.Extension;
        }

        public async Task MergeAsync(string filename)
        {

        }

        /// <summary>
        /// Perform a basic download operation independent of a <see cref="YoutubeDL"/> instance options. 
        /// For more (like progressbar, format selector, etc.), use <see cref="YoutubeDL.DownloadFormat"/>
        /// </summary>
        /// <param name="filename">Filename to download to</param>
        /// <returns></returns>
        new public async Task DownloadAsync(string audiofilename, string videofilename)
        {
            Task a = AudioFormat.DownloadAsync(audiofilename);
            Task v = VideoFormat.DownloadAsync(videofilename);
            await a;
            await v;
        }

        /// <summary>
        /// Perform a basic download operation independent of a <see cref="YoutubeDL"/> instance and its options. 
        /// For more (like progressbar, format selector, etc.), use <see cref="YoutubeDL.DownloadFormat"/>
        /// </summary>
        /// <param name="filename">Filename to download to</param>
        /// <returns></returns>
        new public void Download(string audiofilename, string videofilename)
        {
            AudioFormat.Download(audiofilename);
            VideoFormat.Download(videofilename);
        }
    }*/
}
