using System;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

using YoutubeDL.Models;
using System.Text.RegularExpressions;

namespace YoutubeDL.Postprocessors
{
    internal class FFMpegProgressData : EventArgs
    {
        public FFMpegProgressData()
        {
            StartTime = DateTime.Now;
        }

        public DateTime StartTime { get; set; }
        public TimeSpan Time { get; set; }
        public TimeSpan Duration { get; set; }
    }

    public class FFMpegPP : PostProcessor
    {
        public string Filename { get; set; }
        public string ProbeFilename { get; set; }
        public bool TrackProgress { get; set; } = true;

        private Regex DurationR = new Regex(@"Duration: ([^,]*), ");
        private Regex TimeR = new Regex(@"time=\s*([^ ]*)");

        Dictionary<string, string> ACODECS = new Dictionary<string, string>() 
        {
            {"mp3", "libmp3lame"},
            {"aac", "aac"},
            {"flac", "flac"},
            {"m4a", "aac"},
            {"opus", "libopus"},
            {"vorbis", "libvorbis"},
            {"wav", null},
        };

        public FFMpegPP(bool preferFFMpeg = true) : base()
        {
            /*
            if (preferFFMpeg)
            {
                Filename = "ffmpeg";
                ProbeFilename = "avconf";
            }*/

            Filename = "ffmpeg";
            ProbeFilename = "ffprobe";
        }

        public bool Available 
        { 
            get
            {
                // todo
                return true;
            } 
        }

        public event ProgressEventHandler OnProgress;

        public void Execute(string[] args)
        {
            ProcessStartInfo i = new ProcessStartInfo(Filename, string.Join(' ', args))
            {
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
            };

            using Process p = new Process
            {
                EnableRaisingEvents = true,
                StartInfo = i
            };

            if (TrackProgress)
            {
                FFMpegProgressData data = new FFMpegProgressData();
                p.ErrorDataReceived += (sender, e) => OnFFMpegOutput((Process)sender, e, ref data);
            }
            p.Start();
            if (TrackProgress) p.BeginErrorReadLine();
            p.WaitForExit();

            if (p.ExitCode != 0)
            {
                throw new FFMpegException("FFMpeg Error: Exited with error code " + p.ExitCode);
            }
        }

        public async Task ExecuteAsync(string[] args, CancellationToken token = default)
        {
            ProcessStartInfo i = new ProcessStartInfo(Filename, string.Join(' ', args))
            {
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
            };

            using Process p = new Process
            {
                EnableRaisingEvents = true,
                StartInfo = i
            };

            if (TrackProgress)
            {
                FFMpegProgressData data = new FFMpegProgressData();
                p.ErrorDataReceived += (sender, e) => OnFFMpegOutput((Process)sender, e, ref data);
            }
            p.Start();
            if (TrackProgress) p.BeginErrorReadLine();

            if (await p.WaitForExitAsync(token).ConfigureAwait(false) != 0)
            {
                throw new FFMpegException("FFMpeg Error: Exited with error code " + p.ExitCode);
            }
        }

        private void OnFFMpegOutput(Process sender, DataReceivedEventArgs e, ref FFMpegProgressData data)
        {
            if (e.Data == null) return;
            //Debug.WriteLine("data: " + e.Data);
            Match m = DurationR.Match(e.Data);
            if (m.Success)
            {
                Util.TimeSpanLargeTryParse(m.Groups[1].Value, out TimeSpan time);
                data.Duration = time;
            }

            Match t = TimeR.Match(e.Data);
            if (t.Success)
            {
                Util.TimeSpanLargeTryParse(t.Groups[1].Value, out TimeSpan time);
                data.Time = time;
                OnProgress?.Invoke(this, new ProgressEventArgs(time.Ticks, data.Duration.Ticks, "f", data.StartTime));
            }
        }


        public StreamReader ExecuteProbe(string[] args)
        {
            ProcessStartInfo i = new ProcessStartInfo(ProbeFilename, string.Join(' ', args))
            {
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            using Process p = new Process
            {
                StartInfo = i
            };
            p.Start();
            p.WaitForExit();

            if (p.ExitCode != 0)
            {
                //Debug.WriteLine("Error:" + p.StandardError.ReadToEnd());
                throw new FFMpegException("FFMpeg Error: Exited with error code " + p.ExitCode);
            }

            return p.StandardError;
        }

        public async Task<string> GetAudioCodec(string path)
        {
            var sr = ExecuteProbe(new string[] { "-show_streams", "\"file:" + path + "\"" });
            string codec = null;
            string line;

            while ((line = (await sr.ReadLineAsync()).Trim()) != null)
            {
                if (line.StartsWith("codec_name"))
                {
                    codec = line.Substring("codec_name=".Length).Trim(); ;
                }
                else if (line.StartsWith("codec_type=audio"))
                {
                    return codec;
                }
            }
            return null;
        }

        protected void FFMpegRun(string inputPath, string outputPath, string[] args)
            => FFMpegRun(new string[] { inputPath }, outputPath, args);
        protected void FFMpegRun(string[] inputPaths, string outputPath, string[] args)
        {
            List<string> args2 = new List<string>() { "-y" };
            foreach (string ip in inputPaths)
            {
                args2.Add("-i");
                args2.Add("\"file:" + ip + "\"");
            }
            args2.AddRange(args);
            args2.Add("\"file:" + outputPath + "\"");

            //string cmd = string.Join(' ', args2);

            Execute(args2.ToArray());
            //Debug.WriteLine(output.ReadToEnd());
        }


        protected Task FFMpegRunAsync(string inputPath, string outputPath, string[] args, CancellationToken token = default)
            => FFMpegRunAsync(new string[] { inputPath }, outputPath, args, token);
        protected async Task FFMpegRunAsync(string[] inputPaths, string outputPath, string[] args, CancellationToken token = default)
        {
            List<string> args2 = new List<string>() { "-y" };
            foreach (string ip in inputPaths)
            {
                args2.Add("-i");
                args2.Add("\"file:" + ip + "\"");
            }
            args2.AddRange(args);
            args2.Add("\"file:" + outputPath + "\"");

            await ExecuteAsync(args2.ToArray(), token).ConfigureAwait(false);
        }
    }

    public class FFMpegMergerPP : FFMpegPP, IPostProcessor<CompFormat>
    {
        public bool StrictMerge { get; set; }
        public FFMpegMergerPP(bool strictMerge)
        {
            StrictMerge = strictMerge;
        }

        public void Process(CompFormat format, string filename)
        {
            string[] args = new string[] { "-c", "copy", "-map", "0:v:0", "-map", "1:a:0" };
            if (!format.VideoFormat.IsDownloaded || !format.AudioFormat.IsDownloaded)
            {
                string message = $"The format {format.Name} cant be merged because it is not downloaded";
                LogError(message);
                throw new FFMpegException(message);
            }

            /*
            if (format.AudioFormat.AudioCodec == "opus")
            {
                if (StrictMerge)
                {
                    string message = $"The format {format.Name} cant be merged because opus audio in mp4 is experimental. Disable strict merge to merge anyway.";
                    LogError(message);
                    throw new FFMpegException(message);
                }
                else
                {
                    LogWarning("Opus audio in mp4 is experimental and may not work. Enable strict merge if you dont want to use it.");
                    args = new string[] { "-c", "copy", "-map", "0:v:0", "-map", "1:a:0", "-strict", "-2" };
                }
            }*/

            FFMpegRun(new string[] { format.VideoFormat.FileName, format.AudioFormat.FileName }, filename, args);
            format.FileName = filename;
            format.IsDownloaded = true;
        }

        public async Task ProcessAsync(CompFormat format, string filename, CancellationToken token = default)
        {
            string[] args = new string[] { "-c", "copy", "-map", "0:v:0", "-map", "1:a:0" };
            if (!format.VideoFormat.IsDownloaded || !format.AudioFormat.IsDownloaded)
            {
                string message = $"The format {format.Name} cant be merged because it is not downloaded";
                LogError(message);
                throw new FFMpegException(message);
            }

            if (format.AudioFormat.AudioCodec == "opus")
            {
                if (StrictMerge)
                {
                    string message = $"The format {format.Name} cant be merged because opus audio in mp4 is experimental. Disable strict merge to merge anyway.";
                    LogError(message);
                    throw new FFMpegException(message);
                }
                else
                {
                    LogWarning("Opus audio in mp4 is experimental. Enable strict merge if you dont want to use it.");
                    args = new string[] { "-c", "copy", "-map", "0:v:0", "-map", "1:a:0", "-strict", "-2" };
                }
            }

            await FFMpegRunAsync(new string[] { format.VideoFormat.FileName, format.AudioFormat.FileName }, filename, args, token).ConfigureAwait(false);
            format.FileName = filename;
            format.IsDownloaded = true;
        }
    }

    public class FFMpegConverterPP : FFMpegPP, IPostProcessor<IFormat>
    {
        public string PreferedFormat { get; set; }
        public FFMpegConverterPP(string preferedFormat)
        {
            PreferedFormat = preferedFormat;
        }

        public void Process(IFormat format, string filename)
        {
            if (!format.IsDownloaded)
            {
                string message = $"The format {format.Name} cant be converted because it is not downloaded";
                LogError(message);
                throw new FFMpegException(message);
            }

            if (format.Extension == PreferedFormat)
            {
                string message = $"Not converting video file {format.FileName} - already is in target format {format.Extension}";
                LogError(message);
                throw new FFMpegException(message);
            }

            filename = Path.ChangeExtension(filename, PreferedFormat);
            string[] args;
            if (PreferedFormat == "avi")
                args = new string[] { "-c:v", "libxvid", "-vtag", "XVID" };
            else
                args = new string[] { };

            FFMpegRun(format.FileName, filename, args);
        }

        public async Task ProcessAsync(IFormat format, string filename, CancellationToken token = default)
        {
            if (!format.IsDownloaded)
            {
                string message = $"The format {format.Name} cant be converted because it is not downloaded";
                LogError(message);
                throw new FFMpegException(message);
            }

            if (format.Extension == PreferedFormat)
            {
                string message = $"Not converting video file {format.FileName} - already is in target format {format.Extension}";
                LogError(message);
                throw new FFMpegException(message);
            }

            filename = Path.ChangeExtension(filename, PreferedFormat);
            string[] args;
            if (PreferedFormat == "avi")
                args = new string[] { "-c:v", "libxvid", "-vtag", "XVID" };
            else
                args = new string[] { };

            await FFMpegRunAsync(format.FileName, filename, args, token).ConfigureAwait(false);
        }
    }

    public class FFMpegAudioExtractorPP : FFMpegPP, IPostProcessor<IMuxedFormat>
    {
        Dictionary<string, string> ACODECS = new Dictionary<string, string>()
        {
            {"mp3", "libmp3lame"},
            {"aac", "aac"},
            {"flac", "flac"},
            {"m4a", "aac"},
            {"opus", "libopus"},
            {"vorbis", "libvorbis"},
            {"wav", null},
        };
        public string PreferredCodec { get; set; }
        public int? PreferredQuality { get; set; }
        public FFMpegAudioExtractorPP(string preferredCodec, int? preferredQuality = 0)
        {
            PreferredCodec = preferredCodec;
            PreferredQuality = preferredQuality;
        }
        public void Process(IMuxedFormat format, string filename)
        {
            throw new NotImplementedException();
        }

        public async Task ProcessAsync(IMuxedFormat format, string filename, CancellationToken token = default)
        {
            if (!format.IsDownloaded)
            {
                string message = $"The audio cannot be extracted because {format.Name} is not downloaded";
                LogError(message);
                throw new FFMpegException(message);
            }

            string filecodec = await GetAudioCodec(format.FileName);
            string acodec = null;
            string extension = null;

            List<string> args = new List<string>(3) { "-vn" };

            // todo: simplify "if" statement if possible
            if (filecodec == PreferredCodec || PreferredCodec == "best" || (filecodec == "aac" && PreferredCodec == "m4a"))
            {
                if (filecodec == "aac" && (PreferredCodec == "best" || PreferredCodec == "m4a"))
                {
                    acodec = "copy";
                    extension = "m4a";
                    args.Add("-bsf:a");
                    args.Add("aac_adtstoasc");
                }
                else if ((new List<string>() { "aac", "flac", "mp3", "vorbis", "opus" }).Contains(filecodec))
                {
                    acodec = "copy";
                    extension = filecodec;
                    if (filecodec == "aac")
                    {
                        args.Add("-f");
                        args.Add("adts");
                    }
                    else if (filecodec == "vorbis")
                        extension = "ogg";

                }
                else
                {
                    acodec = "libmp3lame";
                    extension = "mp3";
                    if (PreferredQuality != null)
                    {
                        args.Add(PreferredQuality < 10 ? "-q:a" : "-b:a");
                        args.Add(PreferredQuality.ToString());
                    }
                }
            }
            else
            {
                acodec = ACODECS[PreferredCodec];
                extension = PreferredCodec;
                if (PreferredQuality != null)
                {
                    args.Add(PreferredQuality < 10 && PreferredCodec != "opus" ? "-q:a" : "-b:a");
                    args.Add(PreferredQuality.ToString());
                }

                if (PreferredCodec == "aac")
                {
                    args.Add("-f");
                    args.Add("adts");
                }
                else if (PreferredCodec == "m4a")
                {
                    args.Add("-bsf:a");
                    args.Add("aac_adtstoasc");
                }
                else if (PreferredCodec == "vorbis")
                {
                    extension = "ogg";
                }
                else if (PreferredCodec == "wav")
                {
                    extension = "wav";
                    args.Add("-f");
                    args.Add("waw");
                }
            }

            if (acodec != null)
            {
                args.Insert(1, acodec);
                args.Insert(1, "-acodec");
            }

            filename = Path.ChangeExtension(filename, extension);

            await FFMpegRunAsync(format.FileName, filename, args.ToArray());
        }
    }

    public class FFMpegFixupAspectRatioPP : FFMpegPP, IPostProcessor<IVideoFormat>
    {
        public void Process(IVideoFormat format, string filename)
        {
            string[] args = new string[] { "-c", "copy", "-aspect", format.StretchedRatio.ToString() };
            if (!format.IsDownloaded)
            {
                string message = $"The format {format.Name} cant be fixed because it is not downloaded";
                LogError(message);
                throw new FFMpegException(message);
            }

            if (format.StretchedRatio == null || format.StretchedRatio == 1) return;

            string tempfile = format.FileName + ".temp";

            FFMpegRunAsync(format.FileName, tempfile, args);
            if (File.Exists(filename)) File.Delete(format.FileName);
            File.Move(tempfile, filename);
        }

        public async Task ProcessAsync(IVideoFormat format, string filename, CancellationToken token = default)
        {
            string[] args = new string[] { "-c", "copy", "-aspect", format.StretchedRatio.ToString() };
            if (!format.IsDownloaded)
            {
                string message = $"The format {format.Name} cant be fixed because it is not downloaded";
                LogError(message);
                throw new FFMpegException(message);
            }

            if (format.StretchedRatio == null || format.StretchedRatio == 1) return;

            string tempfile = format.FileName + ".temp";

            await FFMpegRunAsync(format.FileName, tempfile, args, token).ConfigureAwait(false);
            if (File.Exists(filename)) File.Delete(format.FileName);
            File.Move(tempfile, filename);
        }
    }

    public class FFMpegFixupM4APP : FFMpegPP, IPostProcessor<IAudioFormat>
    {
        public void Process(IAudioFormat format, string filename)
        {
            string[] args = new string[] { "-c", "copy", "-f", "mp4" };
            if (!format.IsDownloaded) return; // || format.Container == m4a_dash

            string tempfile = format.FileName + ".temp";

            FFMpegRunAsync(format.FileName, tempfile, args);
            if (File.Exists(filename)) File.Delete(format.FileName);
            File.Move(tempfile, filename);
        }

        public async Task ProcessAsync(IAudioFormat format, string filename, CancellationToken token = default)
        {
            string[] args = new string[] { "-c", "copy", "-f", "mp4" };
            if (!format.IsDownloaded) return; // || format.Container == m4a_dash

            string tempfile = format.FileName + ".temp";

            await FFMpegRunAsync(format.FileName, tempfile, args, token).ConfigureAwait(false);
            if (File.Exists(filename)) File.Delete(format.FileName);
            File.Move(tempfile, filename);
        }
    }

    public class FFMpegFixupM3U8PP : FFMpegPP, IPostProcessor<IFormat>
    {
        public void Process(IFormat format, string filename)
        {
            string[] args = new string[] { "-c", "copy", "-f", "mp4", "-bsf:a", "aac_adtstoasc" };
            if (!format.IsDownloaded || GetAudioCodec(format.FileName).GetAwaiter().GetResult() != "aac") return;

            string tempfile = format.FileName + ".temp";

            FFMpegRunAsync(format.FileName, tempfile, args);
            if (File.Exists(filename)) File.Delete(format.FileName);
            File.Move(tempfile, filename);
        }

        public async Task ProcessAsync(IFormat format, string filename, CancellationToken token = default)
        {
            string[] args = new string[] { "-c", "copy", "-f", "mp4", "-bsf:a", "aac_adtstoasc" };
            if (!format.IsDownloaded || await GetAudioCodec(format.FileName) != "aac") return;

            string tempfile = format.FileName + ".temp";

            await FFMpegRunAsync(format.FileName, tempfile, args, token).ConfigureAwait(false);
            if (File.Exists(filename)) File.Delete(format.FileName);
            File.Move(tempfile, filename);
        }
    }
}
