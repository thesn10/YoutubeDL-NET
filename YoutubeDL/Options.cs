using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Linq;

namespace YoutubeDL
{
    public class YoutubeDLOptions : Dictionary<string,object>
    {
        #region YOUTUBE DL OPTIONS
        /// <summary>
        /// Username for authentication purposes.
        /// </summary>
        [YTDLMeta("username", "u", Description = @"Username for authentication purposes.")]
        public string Username { get; set; }
        /// <summary>
        /// Password for authentication purposes.
        /// </summary>
        [YTDLMeta("password", "p", Description = @"Password for authentication purposes.")]
        public string Password { get; set; }
        /// <summary>
        /// Password accessing a video.
        /// </summary>
        [YTDLMeta("videopassword", Description = @"Password accessing a video.")]
        public string VideoPassword { get; set; }

        /// <summary>
        /// Adobe Pass multiple-system operator identifier.
        /// </summary>
        [YTDLMeta("ap_mso", Description = @"Adobe Pass multiple-system operator identifier.")]
        public string AP_MSO_ID { get; set; }
        /// <summary>
        /// Multiple-system operator account username.
        /// </summary>
        [YTDLMeta("ap_username", Description = @"Multiple-system operator account username.")]
        public string AP_MSO_Username { get; set; }
        /// <summary>
        /// Multiple-system operator account password.
        /// </summary>
        [YTDLMeta("ap_password", Description = @"Multiple-system operator account password.")]
        public string AP_MSO_Passoword { get; set; }
        /// <summary>
        /// Use netrc for authentication instead.
        /// </summary>
        [YTDLMeta("usenetrc", "n", Description = @"Use netrc for authentication instead.")]
        public bool UseNetRC { get; set; }
        /// <summary>
        /// Print additional info to stdout.
        /// </summary>
        [YTDLMeta("verbose", "v", Description = @"Print additional info to stdout.")]
        public bool Verbose { get; set; }
        /// <summary>
        /// Do not print messages to stdout.
        /// </summary>
        [YTDLMeta("quiet", "q", Description = @"Do not print messages to stdout.")]
        public bool Quiet { get; set; } = false;
        /// <summary>
        /// Do not print out anything for warnings.
        /// </summary>
        [YTDLMeta("no_warnings", Description = @"Do not print out anything for warnings.")]
        public bool NoWarnings { get; set; }

        /// <summary>
        /// Force printing final URL.
        /// </summary>
        [YTDLMeta("forceurl", Description = @"Force printing final URL.")]
        public bool ForceUrl { get; set; } = false;

        /// <summary>
        /// Force printing title.
        /// </summary>
        [YTDLMeta("forcetitle", Description = @"Force printing title.")]
        public bool ForceTitle { get; set; } = false;
        /// <summary>
        /// Force printing ID.
        /// </summary>
        [YTDLMeta("forceid", Description = @"Force printing ID.")]
        public bool ForceID { get; set; } = false;
        /// <summary>
        /// Force printing thumbnail URL.
        /// </summary>
        [YTDLMeta("forcethumbnail", Description = @"Force printing thumbnail URL.")]
        public bool ForceThumbnail { get; set; } = false;
        /// <summary>
        /// Force printing description.
        /// </summary>
        [YTDLMeta("forcedescription", Description = @"Force printing description.")]
        public bool ForceDescription { get; set; } = false;
        /// <summary>
        /// Force printing final filename.
        /// </summary>
        [YTDLMeta("forcefilename", Description = @"Force printing final filename.")]
        public bool ForceFilename { get; set; } = false;
        /// <summary>
        /// Force printing duration.
        /// </summary>
        [YTDLMeta("forceduration", Description = @"Force printing duration.")]
        public bool ForceDuration { get; set; } = false;
        /// <summary>
        /// Force printing format.
        /// </summary>
        [YTDLMeta("forceformat", Description = @"Force printing format.")]
        public bool ForceFormat { get; set; } = false;
        /// <summary>
        /// Force printing <see cref="InfoDict"/> as JSON.
        /// </summary>
        [YTDLMeta("forcejson")]
        public bool ForceJson { get; set; } = false;
        /// <summary>
        /// Force printing the <see cref="InfoDict"/>
        /// of the whole playlist (or video) as a single JSON line.
        /// </summary>
        [YTDLMeta("dump_single_json")]
        public bool DumpSingleJson { get; set; }
        /// <summary>
        /// Do not download the video files.
        /// </summary>
        [YTDLMeta("simulate", "s", Description = @"Do not download the video files.")]
        public bool Simulate { get; set; } = false;
        /// <summary>
        /// Video format code. See options.py for more information.
        /// </summary>
        [YTDLMeta("format", "f", Description = @"Video format code. See options.py for more information.")]
        public string Format { get; set; } = "bestvideo+bestaudio/best";
        /// <summary>
        /// Template for output names.
        /// </summary>
        [YTDLMeta("outtmpl", "o", Description = @"Template for output names.")]
        public string OutTemplate { get; set; } = "{title}-{id}-{format_id}.{ext}";
        /// <summary>
        /// Do not allow "&" and spaces in file names
        /// </summary>
        [YTDLMeta("restrictfilenames", Description = "Do not allow \"&\" and spaces in file names")]
        public bool RestrictFileNames { get; set; }
        /// <summary>
        /// Do not stop on download errors.
        /// </summary>
        [YTDLMeta("ignoreerrors", Description = @"Do not stop on download errors.")]
        public bool IgnoreErrors { get; set; }
        /// <summary>
        /// Force downloader to use the generic extractor
        /// </summary>
        [YTDLMeta("force_generic_extractor", Description = @"Force downloader to use the generic extractor")]
        public bool ForceGenericExtractor { get; set; }
        /// <summary>
        /// Prevent overwriting files.
        /// </summary>
        [YTDLMeta("nooverwrites", "w", Description = @"Prevent overwriting files.")]
        public bool NoOverwrites { get; set; }
        /// <summary>
        /// Playlist item to start at.
        /// </summary>
        [YTDLMeta("playliststart", Description = @"Playlist item to start at.")]
        public int PlaylistStart { get; set; } = 1;
        /// <summary>
        /// Playlist item to end at.
        /// </summary>
        [YTDLMeta("playlistend", Description = @"Playlist item to end at.")]
        public int? PlaylistEnd { get; set; } = null;
        /// <summary>
        /// Specific indices of playlist to download.
        /// </summary>
        [YTDLMeta("playlist_items", Description = @"Specific indices of playlist to download.")]
        public int[] PlaylistItems { get; set; }
        /// <summary>
        /// Download playlist items in reverse order.
        /// </summary>
        [YTDLMeta("playlistreverse", Description = @"Download playlist items in reverse order.")]
        public bool PlaylistReverse { get; set; }
        /// <summary>
        /// Download playlist items in random order.
        /// </summary>
        [YTDLMeta("playlistrandom", Description = @"Download playlist items in random order.")]
        public bool PlaylistRandom { get; set; }
        /// <summary>
        /// Download only matching titles.
        /// </summary>
        [YTDLMeta("matchtitle", Description = @"Download only matching titles.")]
        public string MatchTitle { get; set; }
        /// <summary>
        /// Reject downloads for matching titles.
        /// </summary>
        [YTDLMeta("rejecttitle", Description = @"Reject downloads for matching titles.")]
        public string RejectTitle { get; set; }
        /// <summary>
        /// Log messages to a logging. Logger instance.
        /// </summary>
        [YTDLMeta("logger", Description = @"Log messages to a logging. Logger instance.")]
        public dynamic Logger { get; set; }
        /// <summary>
        /// Log messages to stderr instead of stdout.
        /// </summary>
        [YTDLMeta("logtostderr", Description = @"Log messages to stderr instead of stdout.")]
        public bool LogToStdERR { get; set; }
        /// <summary>
        /// Write the video description to a .description file
        /// </summary>
        [YTDLMeta("writedescription", Description = @"Write the video description to a .description file")]
        public bool WriteDescription { get; set; } = false;
        /// <summary>
        /// Write the video description to a .info.json file
        /// </summary>
        [YTDLMeta("writeinfojson", Description = @"Write the video description to a .info.json file")]
        public bool WriteInfoJson { get; set; } = false;
        /// <summary>
        /// Write the video description to a .annotations.xml file
        /// </summary>
        [YTDLMeta("writeannotations", Description = @"Write the video description to a .annotations.xml file")]
        public bool WriteAnnotations { get; set; } = false;
        /// <summary>
        /// Write the thumbnail image to a file
        /// </summary>
        [YTDLMeta("writethumbnail", Description = @"Write the thumbnail image to a file")]
        public bool WriteThumbnail { get; set; } = false;
        /// <summary>
        /// Write all thumbnail formats to files
        /// </summary>
        [YTDLMeta("write_all_thumbnails", Description = @"Write all thumbnail formats to files")]
        public bool WriteAllThumbnails { get; set; } = false;
        /// <summary>
        /// Write the video subtitles to a file
        /// </summary>
        [YTDLMeta("writesubtitles", Description = @"Write the video subtitles to a file")]
        public bool WriteSubtitles { get; set; } = false;
        /// <summary>
        /// Write the automatically generated subtitles to a file
        /// </summary>
        [YTDLMeta("writeautomaticsub", Description = @"Write the automatically generated subtitles to a file")]
        public bool WriteAutomaticSub { get; set; } = false;
        /// <summary>
        /// Downloads all the subtitles of the video
        /// (requires writesubtitles or writeautomaticsub)
        /// </summary>
        [YTDLMeta("allsubtitles", Description = @"Downloads all the subtitles of the video
         (requires writesubtitles or writeautomaticsub)")]
        public bool AllSubtitles { get; set; } = false;
        /// <summary>
        /// Lists all available subtitles for the video
        /// </summary>
        [YTDLMeta("listsubtitles", Description = @"Lists all available subtitles for the video")]
        public bool ListSubtitles { get; set; }
        /// <summary>
        /// The format code for subtitles
        /// </summary>
        [YTDLMeta("subtitlesformat", Description = @"The format code for subtitles")]
        public string SubtitlesFormat { get; set; } = "best";
        /// <summary>
        /// List of languages of the subtitles to download
        /// </summary>
        [YTDLMeta("subtitleslangs", Description = @"List of languages of the subtitles to download")]
        public IList<string> SubtitlesLangs { get; set; } = null;
        /// <summary>
        /// Keep the video file after post-processing
        /// </summary>
        [YTDLMeta("keepvideo", "k", Description = @"Keep the video file after post-processing")]
        public bool KeepVideo { get; set; }
        /// <summary>
        /// Download only if the upload_date is in the <see cref="DateTime"/> range.
        /// </summary>
        [YTDLMeta("daterange")]
        public Tuple<DateTime, DateTime> DateRange { get; set; }
        /// <summary>
        /// Skip the actual download of the video file
        /// </summary>
        [YTDLMeta("skip_download", Description = @"Skip the actual download of the video file")]
        public bool SkipDownload { get; set; } = false;
        /// <summary>
        /// Location of the cache files in the filesystem.
        /// <see cref="null"/> to disable filesystem cache.
        /// </summary>
        [YTDLMeta("cachedir")]
        public string CacheDir { get; set; }
        /// <summary>
        /// Download single video instead of a playlist if in doubt.
        /// </summary>
        [YTDLMeta("noplaylist", Description = @"Download single video instead of a playlist if in doubt.")]
        public bool NoPlaylist { get; set; }
        /// <summary>
        /// An integer representing the user's age in years.
        /// Unsuitable videos for the given age are skipped.
        /// </summary>
        [YTDLMeta("age_limit", Description = @"An integer representing the user's age in years.
         Unsuitable videos for the given age are skipped.")]
        public int AgeLimit { get; set; }
        /// <summary>
        /// An integer representing the minimum view count the video
        /// must have in order to not be skipped.
        /// Videos without view count information are always
        /// downloaded. None for no limit.
        /// </summary>
        [YTDLMeta("min_views", Description = @"An integer representing the minimum view count the video
         must have in order to not be skipped.
         Videos without view count information are always
         downloaded. None for no limit.")]
        public int? MinViews { get; set; }
        /// <summary>
        /// An integer representing the maximum view count.
        /// Videos that are more popular than that are not downloaded.
        /// Videos without view count information are always
        /// downloaded. None for no limit.
        /// </summary>
        [YTDLMeta("max_views", Description = @"An integer representing the maximum view count.
         Videos that are more popular than that are not downloaded.
         Videos without view count information are always
         downloaded. None for no limit.")]
        public int? MaxViews { get; set; }
        /// <summary>
        /// An integer representing the maximum Download count. None for no limit.
        /// </summary>
        [YTDLMeta("max_downloads", Description = @"An integer representing the maximum Download count. None for no limit.")]
        public int? MaxDownloads { get; set; }
        /// <summary>
        /// File name of a file where all downloads are recorded.
        /// Videos already present in the file are not downloaded
        /// again.
        /// </summary>
        [YTDLMeta("download_archive", Description = @"File name of a file where all downloads are recorded.
         Videos already present in the file are not downloaded
         again.")]
        public string DownloadArchive { get; set; }
        /// <summary>
        /// File name where cookies should be read from and dumped to.
        /// </summary>
        [YTDLMeta("cookiefile", Description = @"File name where cookies should be read from and dumped to.")]
        public string CookieFile { get; set; }
        /// <summary>
        /// Do not verify SSL certificates
        /// </summary>
        [YTDLMeta("nocheckcertificate", Description = @"Do not verify SSL certificates")]
        public bool NoCheckCertificate { get; set; }
        /// <summary>
        /// Use HTTP instead of HTTPS to retrieve information.
        /// At the moment, this is only supported by YouTube.
        /// </summary>
        [YTDLMeta("prefer_insecure", Description = @"Use HTTP instead of HTTPS to retrieve information.
         At the moment, this is only supported by YouTube.")]
        public bool PreferInscure { get; set; }
        /// <summary>
        /// URL of the proxy server to use
        /// </summary>
        [YTDLMeta("proxy", Description = @"URL of the proxy server to use")]
        public string Proxy { get; set; }
        /// <summary>
        /// URL of the proxy to use for IP address verification
        /// on geo-restricted sites.
        /// </summary>
        [YTDLMeta("geo_verification_proxy", Description = @"URL of the proxy to use for IP address verification
         on geo-restricted sites.")]
        public string GeoVerificationProxy { get; set; }
        /// <summary>
        /// Time to wait for unresponsive hosts
        /// </summary>
        [YTDLMeta("socket_timeout", Description = @"Time to wait for unresponsive hosts")]
        public TimeSpan SocketTimeout { get; set; }
        /// <summary>
        /// Work around buggy terminals without bidirectional text
        /// support, using fridibi
        /// </summary>
        [YTDLMeta("bidi_workaround", Description = @"Work around buggy terminals without bidirectional text
         support, using fridibi")]
        public bool BiDiWorkaround { get; set; }
        /// <summary>
        /// Print out sent and received HTTP traffic
        /// </summary>
        [YTDLMeta("debug_printtraffic", Description = @"Print out sent and received HTTP traffic")]
        public bool DebugPrintTraffic { get; set; }
        /// <summary>
        /// Download ads as well
        /// </summary>
        [YTDLMeta("include_ads", Description = @"Download ads as well")]
        public bool IncludeAds { get; set; }
        /// <summary>
        /// Prepend this string if an input url is not valid.
        /// 'auto' for elaborate guessing
        /// </summary>
        [YTDLMeta("default_search", Description = @"Prepend this string if an input url is not valid.
         'auto' for elaborate guessing")]
        public string DefaultSearch { get; set; }
        /// <summary>
        /// Use this encoding instead of the system-specified.
        /// </summary>
        [YTDLMeta("encoding", Description = @"Use this encoding instead of the system-specified.")]
        public Encoding Encoding { get; set; }
        /// <summary>
        /// Do not resolve URLs, return the immediate result.
        /// Pass in 'in_playlist' to only show this behavior for
        /// playlist items.
        /// </summary>
        [YTDLMeta("extract_flat", Description = @"Do not resolve URLs, return the immediate result.
         Pass in 'in_playlist' to only show this behavior for
         playlist items.")]
        public string ExtractFlat { get; set; }
        /// <summary>
        /// A list of dictionaries, each with an entry
        /// * key:  The name of the postprocessor. See
        /// youtube_dl/postprocessor/__init__.py for a list.
        /// as well as any further keyword arguments for the
        /// postprocessor.
        /// </summary>
        [YTDLMeta("postprocessors", Description = @"A list of dictionaries, each with an entry
         * key:  The name of the postprocessor. See
         youtube_dl/postprocessor/__init__.py for a list.
         as well as any further keyword arguments for the
         postprocessor.")]
        public List<Dictionary<string, object>> PostProcessors { get; set; }
        /// A list of functions that get called on download
        /// progress, with a dictionary with the entries
        /// * status: One of "downloading", "error", or "finished".
        /// Check this first and ignore unknown values.
        /// If status is one of "downloading", or "finished", the
        /// following properties may also be present:
        /// * filename: The final filename (always present)
        /// * tmpfilename: The filename we're currently writing to
        /// * downloaded_bytes: Bytes on disk
        /// * total_bytes: Size of the whole file, None if unknown
        /// * total_bytes_estimate: Guess of the eventual file size,
        ///                         None if unavailable.
        /// * elapsed: The number of seconds since download started.
        /// * eta: The estimated time in seconds, None if unknown
        /// * speed: The download speed in bytes/second, None if
        ///          unknown
        /// * fragment_index: The counter of the currently
        ///                   downloaded video fragment.
        /// * fragment_count: The number of fragments (= individual
        ///                   files that will be merged)
        /// Progress hooks are guaranteed to be called at least once
        /// (with status "finished") if the download is successful.
        [YTDLMeta("progress_hooks")]
        public List<Action<Dictionary<string, object>>> ProgressHooks { get; set; }
        /// <summary>
        /// Extension to use when merging formats.
        /// </summary>
        [YTDLMeta("merge_output_format", Description = @"Extension to use when merging formats.")]
        public string MergeOutputFormat { get; set; } = null;
        /// <summary>
        /// Automatically correct known faults of the file.
        /// One of: 
        /// <list type="bullet">  
        /// <listheader>  
        ///     <term>never</term>  
        ///     <description>do nothing</description>  
        /// </listheader>  
        /// <item>  
        ///     <term>warn</term>  
        ///     <description>only emit a warning</description>  
        /// </item> 
        /// <item>  
        ///     <term>detect_or_warn</term>  
        ///     <description>check whether we can do anything about it, 
        ///     warn otherwise (default)</description>  
        /// </item>
        /// </list> 
        /// </summary>
        [YTDLMeta("fixup")]
        public FixupPolicy Fixup { get; set; } = FixupPolicy.Warn;
        /// <summary>
        /// Client-side IP address to bind to.
        /// </summary>
        [YTDLMeta("source_address", Description = @"Client-side IP address to bind to.")]
        public string SourceAddress { get; set; }
        /// <summary>
        /// Boolean, true if we are allowed to contact the
        /// youtube-dl servers for debugging.
        /// </summary>
        [YTDLMeta("call_home", Description = @"Boolean, true if we are allowed to contact the
         youtube-dl servers for debugging.")]
        public bool CallHome { get; set; }
        /// <summary>
        /// Number of seconds to sleep before each download when
        /// used alone or a lower bound of a range for randomized
        /// sleep before each download(minimum possible number
        /// of seconds to sleep) when used along with
        /// max_sleep_interval.
        /// </summary>
        [YTDLMeta("sleep_inteval", Description = @"Number of seconds to sleep before each download when
         used alone or a lower bound of a range for randomized
         sleep before each download(minimum possible number
         of seconds to sleep) when used along with
         max_sleep_interval.")]
        public TimeSpan SleepInterval { get; set; }
        /// <summary>
        /// Upper bound of a range for randomized sleep before each
        /// download (maximum possible number of seconds to sleep).
        /// Must only be used along with sleep_interval.
        /// Actual sleep time will be a random float from range
        /// [sleep_interval; max_sleep_interval].
        /// </summary>
        [YTDLMeta("max_sleep_inteval", Description = @"Upper bound of a range for randomized sleep before each
         download (maximum possible number of seconds to sleep).
         Must only be used along with sleep_interval.
         Actual sleep time will be a random float from range
         [sleep_interval; max_sleep_interval].")]
        public TimeSpan MaxSleepInterval { get; set; }
        /// <summary>
        /// Print an overview of available video formats and exit.
        /// </summary>
        [YTDLMeta("listformats", "F", Description = @"Print an overview of available video formats and exit.")]
        public bool ListFormats { get; set; }
        /// <summary>
        /// Print a table of all thumbnails and exit.
        /// </summary>
        [YTDLMeta("list_thumbnails", Description = @"Print a table of all thumbnails and exit.")]
        public bool ListThumbnails { get; set; }
        /// <summary>
        /// A function that gets called with the info_dict of
        /// every video.
        /// If it returns a message, the video is ignored.
        /// If it returns None, the video is downloaded.
        /// match_filter_func in utils.py is one example for this.
        /// </summary>
        [YTDLMeta("match_filter", Description = @"A function that gets called with the info_dict of
         every video.
         If it returns a message, the video is ignored.
         If it returns None, the video is downloaded.
         match_filter_func in utils.py is one example for this.")]
        public Action MatchFilter { get; set; }
        /// <summary>
        /// Do not emit color codes in output.
        /// </summary>
        [YTDLMeta("no_color", Description = @"Do not emit color codes in output.")]
        public bool NoColor { get; set; } = false;
        /// <summary>
        /// Bypass geographic restriction via faking X-Forwarded-For
        /// HTTP header
        /// </summary>
        [YTDLMeta("geo_bypass", Description = @"Bypass geographic restriction via faking X-Forwarded-For
         HTTP header")]
        public bool GeoBypass { get; set; }
        /// <summary>
        /// Two-letter ISO 3166-2 country code that will be used for
        /// explicit geographic restriction bypassing via faking
        /// X-Forwarded-For HTTP header
        /// </summary>
        [YTDLMeta("geo_bypass_country", Description = @"Two-letter ISO 3166-2 country code that will be used for
         explicit geographic restriction bypassing via faking
         X-Forwarded-For HTTP header")]
        public bool GeoBypassCountry { get; set; }
        /// <summary>
        /// IP range in CIDR notation that will be used similarly to
        /// geo_bypass_country
        /// </summary>
        [YTDLMeta("geo_bypass_ip_block", Description = @"IP range in CIDR notation that will be used similarly to
         geo_bypass_country")]
        public bool GeoBypassIpBlock { get; set; }
        /// <summary>
        /// Executable of the external downloader to call.
        /// None or unset for standard (built-in) downloader.
        /// </summary>
        [YTDLMeta("external_downloader", Description = @"Executable of the external downloader to call.
         None or unset for standard (built-in) downloader.")]
        public bool ExternalDownloader { get; set; }
        /// <summary>
        /// Use the native HLS downloader instead of ffmpeg/avconv
        /// if True, otherwise use ffmpeg/avconv if False, otherwise
        /// use downloader suggested by extractor if None.
        /// </summary>
        [YTDLMeta("hls_prefer_native", Description = @"Use the native HLS downloader instead of ffmpeg/avconv
         if True, otherwise use ffmpeg/avconv if False, otherwise
         use downloader suggested by extractor if None.")]
        public bool HlsPreferNative { get; set; }
        /// <summary>
        /// Options of the File Downloader
        /// </summary>
        public DownloaderOptions DownloaderOptions { get; set; }
        /// <summary>
        /// Any additional Extractor Options
        /// </summary>
        public ExtractorOptions ExtractorOptions { get; set; }

        #endregion

        #region YOUTUBE DL .NET SPECIFIC OPTIONS
        /// <summary>
        /// Faster loading of python extractors (experimental)
        /// </summary>
        public bool LazyLoad { get; set; } = true;
        #endregion


        public YoutubeDLOptions()
        {
            DownloaderOptions = new DownloaderOptions();
            ExtractorOptions = new ExtractorOptions();
        }

        public object GetOption(string name)
        {
            return this.GetType().GetProperty(name).GetValue(this);
        }

        public void SetOptions(Dictionary<string,object> options)
        {
            foreach (PropertyInfo pInfo in this.GetType().GetProperties(BindingFlags.Public))
            {
                var pythonNameAttr = (YTDLMetaAttribute)pInfo.GetCustomAttribute(typeof(YTDLMetaAttribute));
                if (pythonNameAttr != null) 
                {
                    if (options.TryGetValue(pInfo.Name, out object ovalue))
                    {
                        pInfo.SetValue(this, ovalue);
                    }

                    if (options.TryGetValue(pythonNameAttr.PythonName, out object ovalue2))
                    {
                        pInfo.SetValue(this, ovalue2);
                    }
                }
                else
                {
                    if (options.TryGetValue(pInfo.Name, out object ovalue))
                    {
                        pInfo.SetValue(this, ovalue);
                    }
                }
            }
        }

        public static string GetPropDescription(string property)
        {
            var props = typeof(YoutubeDLOptions).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var prop = props.FirstOrDefault(x =>
            {
                var attr = x.GetCustomAttribute<YTDLMetaAttribute>();
                return attr != null && (property == x.Name || property == attr.PythonName || property == attr.ArgName);
            });
            return prop == default ? null : prop.GetCustomAttribute<YTDLMetaAttribute>().Description;
        }
    }

    public class ExtractorOptions
    {
        public Dictionary<string, object> AdditionalOptions { get; set; } = new Dictionary<string, object>();

        public ExtractorOptions()
        {

        }
    }

    public class DownloaderOptions
    {
        /// <summary>
        /// Print additional info to stdout.
        /// </summary>
        [YTDLMeta("verbose", Description = @"Print additional info to stdout.")]
        public bool Verbose { get; set; }
        /// <summary>
        /// Do not print messages to stdout.
        /// </summary>
        [YTDLMeta("quiet", Description = @"Do not print messages to stdout.")]
        public bool Quiet { get; set; }
        /// <summary>
        /// Download speed limit, in bytes/sec.
        /// </summary>
        [YTDLMeta("ratelimit", Description = @"Download speed limit, in bytes/sec.")]
        public int RateLimit { get; set; }
        /// <summary>
        /// Number of times to retry for HTTP error 5xx.
        /// </summary>
        [YTDLMeta("retries", Description = @"Number of times to retry for HTTP error 5xx.")]
        public int Retries { get; set; }
        /// <summary>
        /// Size of download buffer in bytes.
        /// </summary>
        [YTDLMeta("buffersize", Description = @"Size of download buffer in bytes.")]
        public int BufferSize { get; set; }
        /// <summary>
        /// Do not automatically resize the download buffer.
        /// </summary>
        [YTDLMeta("noresizebuffer", Description = @"Do not automatically resize the download buffer.")]
        public bool NoResizeBuffer { get; set; }
        /// <summary>
        /// Try to continue downloads if possible.
        /// </summary>
        [YTDLMeta("continuedl", "c", Description = @"Try to continue downloads if possible.")]
        public bool ContinueDL { get; set; }
        /// <summary>
        /// Do not print the progress bar.
        /// </summary>
        [YTDLMeta("noprogress", Description = @"Do not print the progress bar.")]
        public bool NoProgressBar { get; set; }
        /// <summary>
        /// Log messages to stderr instead of stdout.
        /// </summary>
        [YTDLMeta("logtostderr", Description = @"Log messages to stderr instead of stdout.")]
        public bool LogToStdERR { get; set; }
        /// <summary>
        /// Display progress in console window's titlebar.
        /// </summary>
        [YTDLMeta("consoletitle", Description = @"Display progress in console window's titlebar.")]
        public bool ConsoleTilteProgress { get; set; }
        /// <summary>
        /// Do not use temporary .part files.
        /// </summary>
        [YTDLMeta("nopart", Description = @"Do not use temporary .part files.")]
        public bool NoPart { get; set; }
        /// <summary>
        /// Use the Last-modified header to set output file timestamps.
        /// </summary>
        [YTDLMeta("updatetime", Description = @"Use the Last-modified header to set output file timestamps.")]
        public bool UpdateTime { get; set; }
        /// <summary>
        /// Download only first bytes to test the downloader.
        /// </summary>
        [YTDLMeta("test", Description = @"Download only first bytes to test the downloader.")]
        public bool Test { get; set; }
        /// <summary>
        /// Skip files smaller than this size
        /// </summary>
        [YTDLMeta("min_filesize", Description = @"Skip files smaller than this size")]
        public bool MinFilesize { get; set; }
        /// <summary>
        /// Skip files smaller than this size
        /// </summary>
        [YTDLMeta("max_filesize", Description = @"Skip files smaller than this size")]
        public bool MaxFilesize { get; set; }
        /// <summary>
        /// Set ytdl.filesize user xattribute with expected size.
        /// </summary>
        [YTDLMeta("xattr_set_filesize", Description = @"Set ytdl.filesize user xattribute with expected size.")]
        public bool XAttrSetFilesize { get; set; }
        /// <summary>
        /// A list of additional command-line arguments for the
        /// external downloader.
        /// </summary>
        [YTDLMeta("external_downloader_args", Description = @"A list of additional command-line arguments for the
         external downloader.")]
        public List<string> ExternalDownloaderArgs { get; set; }
        /// <summary>
        /// Use the mpegts container for HLS videos.
        /// </summary>
        [YTDLMeta("hls_use_mpegts", Description = @"Use the mpegts container for HLS videos.")]
        public List<string> HLS_Use_MPEGTS { get; set; }
        /// <summary>
        /// Size of a chunk for chunk-based HTTP downloading. May be
        /// useful for bypassing bandwidth throttling imposed by
        /// a webserver (experimental)
        /// </summary>
        [YTDLMeta("http_chunk_size", Description = @"Size of a chunk for chunk-based HTTP downloading. May be
         useful for bypassing bandwidth throttling imposed by
         a webserver (experimental)")]
        public long HTTPChunkSize { get; set; }

        public DownloaderOptions()
        {

        }
    }
}
