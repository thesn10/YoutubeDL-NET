using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;

namespace YoutubeDL.Models
{
    public class FormatCollection : ReadOnlyCollection<IFormat>
    {
        public FormatCollection(IList<IFormat> formats) : base(formats)
        {
        }

        public FormatCollection(IEnumerable<IFormat> formats) : base(formats.ToList())
        {
        }

        public IList<IFormat> SelectFormats(string format_spec, string mergeOutputFormat = null)
            => FormatParser.SelectFormats(Items, format_spec, mergeOutputFormat);

        public IEnumerable<IAudioFormat> WithAudio() => Items.Where(x => x is IAudioFormat).Select(x => (IAudioFormat)x);
        public IEnumerable<IVideoFormat> WithVideo() => Items.Where(x => x is IVideoFormat).Select(x => (IVideoFormat)x);
        public IEnumerable<IAudioFormat> GetAudioOnlyFormats() => WithAudio().Where(x => !(x is IVideoFormat));
        public IEnumerable<IVideoFormat> GetVideoOnlyFormats() => WithVideo().Where(x => !(x is IAudioFormat));
        public IEnumerable<IMuxedFormat> GetMuxedFormats() => Items.Where(x => x is IMuxedFormat).Select(x => (IMuxedFormat)x);

        public IMuxedFormat MuxedWithBestResolution() => GetMuxedFormats().OrderByDescending(x => x.Height).First();
        public IVideoFormat WithBestVideoResolution() => WithVideo().OrderByDescending(x => x.Height).First();
        public IAudioFormat WithBestAudioBitrate() => WithAudio().OrderByDescending(x => x.AudioBitrate).First();
    }

    public class ThumbnailCollection : ReadOnlyCollection<Thumbnail>
    {
        public ThumbnailCollection(IList<Thumbnail> list) : base(list)
        {
        }

        public Thumbnail WithBestResolution() => Items.OrderByDescending(x => x.Height).First();
    }

    public class SubtitleCollection : ReadOnlyCollection<Subtitle>
    {
        public SubtitleCollection(IList<Subtitle> list) : base(list)
        {
        }

        public Subtitle WithSystemLanguage() => WithLanguage(CultureInfo.CurrentCulture);
        public Subtitle WithLanguage(string languageCode) => Items.First(x => x.Name == languageCode);
        public Subtitle WithLanguage(CultureInfo culture) => Items.First(x => x.Name == culture.TwoLetterISOLanguageName);
    }
}
