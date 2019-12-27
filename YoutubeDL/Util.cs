using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices;
using System.Linq;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace YoutubeDL
{
    class Util
    {
        public static string SanitizeUrl(string url)
        {
            // Prepend protocol-less URLs with `http:` scheme in order to mitigate
            // the number of unwanted failures due to missing protocol
            if (url.StartsWith("//"))
            {
                return "http:" + url;
            }
            // Fix some common typos seen so far
            Dictionary<string,string> COMMON_TYPOS = new Dictionary<string, string>
            {
                { @"^httpss://", @"https://" },
                { @"^rmtp([es]?)://", @"rtmp\1://"}
            };

            foreach (var kv in COMMON_TYPOS)
            {
                if (Regex.IsMatch(url, kv.Key))
                {
                    return Regex.Replace(url, kv.Key, kv.Value);
                }
            }
            return url;
        }

        public static void Shuffle<T>(T[] array)
        {
            var rng = new Random();
            int n = array.Length;
            while (n > 1)
            {
                int k = rng.Next(n--);
                T temp = array[n];
                array[n] = array[k];
                array[k] = temp;
            }
        }

        public static void Shuffle<T>(IList<T> list)
        {
            var rng = new Random();
            int n = list.Count;
            while (n > 1)
            {
                int k = rng.Next(n--);
                T temp = list[n];
                list[n] = list[k];
                list[k] = temp;
            }
        }

        public static IEnumerable<Tuple<T, T>> Product<T>(IEnumerable<T> a, IEnumerable<T> b) where T : class
        {
            List<Tuple<T, T>> result = new List<Tuple<T, T>>();

            foreach (T t1 in a)
            {
                foreach (T t2 in b)
                    result.Add(Tuple.Create<T, T>(t1, t2));
            }

            return result;
        }

        public static string DetermineProtocol(string url)
        {
            if (url.StartsWith("rtmp"))
                return "rtmp";
            else if (url.StartsWith("mms"))
                return "mms";
            else if (url.StartsWith("rtsp"))
                return "rtsp";

            var ext = "meh";//determine_ext(url)
            if (ext == "m3u8")
                return "m3u8";
            else if (ext == "f4m")
                return "f4m";

            return new Uri(url).Scheme;
        }

        public static int? ParseFilesize(string s)
        {
            if (s == null) return null;

            Dictionary<string, int> _UNIT_TABLE = new Dictionary<string, int>() {
                { "B", 1},
                {"b", 1},
                {"bytes", 1 },
                {"KiB", 1024},
                {"KB", 1000},
                {"kB", 1024},
                {"Kb", 1000},
                {"kb", 1000},
                {"kilobytes", 1000},
                {"kibibytes", 1024},
                {"MiB", (int)Math.Pow(1024, 2)},
                {"MB", (int)Math.Pow(1000, 2)},
                {"mB", (int)Math.Pow(1024, 2)},
                {"Mb", (int)Math.Pow(1000, 2)},
                {"mb", (int)Math.Pow(1000, 2)},
                {"megabytes", (int)Math.Pow(1000, 2)},
                {"mebibytes", (int)Math.Pow(1024, 2)},
                {"GiB", (int)Math.Pow(1024,3)},
                {"GB", (int)Math.Pow(1000,3)},
                {"gB", (int)Math.Pow(1024,3)},
                {"Gb", (int)Math.Pow(1000,3)},
                {"gb", (int)Math.Pow(1000,3)},
                {"gigabytes", (int)Math.Pow(1000,3)},
                {"gibibytes", (int)Math.Pow(1024,3)},
                {"TiB", (int)Math.Pow(1024,4)},
                {"TB", (int)Math.Pow(1000,4)},
                {"tB", (int)Math.Pow(1024,4)},
                {"Tb", (int)Math.Pow(1000,4)},
                {"tb", (int)Math.Pow(1000,4)},
                {"terabytes", (int)Math.Pow(1000,4)},
                {"tebibytes", (int)Math.Pow(1024,4)},
                {"PiB", (int)Math.Pow(1024,5)},
                {"PB", (int)Math.Pow(1000,5)},
                {"pB", (int)Math.Pow(1024,5)},
                {"Pb", (int)Math.Pow(1000,5)},
                {"pb", (int)Math.Pow(1000,5)},
                {"petabytes", (int)Math.Pow(1000,5)},
                {"pebibytes", (int)Math.Pow(1024,5)},
                {"EiB", (int)Math.Pow(1024,6)},
                {"EB", (int)Math.Pow(1000,6)},
                {"eB", (int)Math.Pow(1024,6)},
                {"Eb", (int)Math.Pow(1000,6)},
                {"eb", (int)Math.Pow(1000,6)},
                {"exabytes", (int)Math.Pow(1000,6)},
                {"exbibytes", (int)Math.Pow(1024,6)},
                {"ZiB", (int)Math.Pow(1024,7)},
                {"ZB", (int)Math.Pow(1000,7)},
                {"zB", (int)Math.Pow(1024,7)},
                {"Zb", (int)Math.Pow(1000,7)},
                {"zb", (int)Math.Pow(1000,7)},
                {"zettabytes", (int)Math.Pow(1000,7)},
                {"zebibytes", (int)Math.Pow(1024,7)},
                {"YiB", (int)Math.Pow(1024,8)},
                {"YB", (int)Math.Pow(1000,8)},
                {"yB", (int)Math.Pow(1024,8)},
                {"Yb", (int)Math.Pow(1000,8)},
                {"yb", (int)Math.Pow(1000,8)},
                {"yottabytes", (int)Math.Pow(1000,8)},
                {"yobibytes", (int)Math.Pow(1024,8)},
            };
            string units_re = string.Join("|", _UNIT_TABLE.Keys.Select(x => Regex.Escape(x)));
            Match m = Regex.Match(string.Format(@"(?P<num>[0-9]+(?:[,.][0-9]*)?)\s*(?P<unit>%s)\b", units_re), s);
            if (!m.Success) return null;
            string num_str = m.Groups["num"].Value.Replace(',', '.');
            int mult = _UNIT_TABLE[m.Groups["unit"].Value];
            if (float.TryParse(num_str, out float num))
            {
                return (int)num * mult;
            }
            return null;
        }

        // Parse timespan string as returned by ffmpeg. No days, allow hours to
        // exceed 23.
        public static bool TimeSpanLargeTryParse(string str, out TimeSpan result)
        {
            result = TimeSpan.Zero;

            // Process hours.
            int hours = 0;
            int start = 0;
            int end = str.IndexOf(':', start);
            if (end < 0)
                return false;
            if (!int.TryParse(str.Substring(start, end - start), out hours))
                return false;

            // Process minutes
            int minutes = 0;
            start = end + 1;
            end = str.IndexOf(':', start);
            if (end < 0)
                return false;
            if (!int.TryParse(str.Substring(start, end - start), out minutes))
                return false;

            // Process seconds
            double seconds = 0.0;
            start = end + 1;
            // ffmpeg doesnt respect the computers culture
            if (!double.TryParse(str.Substring(start), NumberStyles.Number, CultureInfo.InvariantCulture, out seconds))
                return false;

            result = new TimeSpan(0, hours, minutes, 0, (int)Math.Round(seconds * 1000.0));
            return true;
        }

        private const int STD_OUTPUT_HANDLE = -11;
        private const uint ENABLE_VIRTUAL_TERMINAL_PROCESSING = 0x0004;
        private const uint DISABLE_NEWLINE_AUTO_RETURN = 0x0008;

        [DllImport("kernel32.dll")]
        private static extern bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);

        [DllImport("kernel32.dll")]
        private static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GetStdHandle(int nStdHandle);

        public static bool TryEnableANSIColor()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                var version = RuntimeInformation.OSDescription.Split('.');
                var major = version[0];
                var build = int.Parse(version[2]);

                if (major == "Microsoft Windows 10")
                {
                    if (build >= 10586 && build < 14393)
                    {
                        // tecnically, ansi color is supported, but it is too buggy
                        return false;
                    }
                    else if (build > 14393)
                    {
                        var handle = GetStdHandle(STD_OUTPUT_HANDLE);
                        if (!GetConsoleMode(handle, out uint lpMode))
                        {
                            return false;
                        }

                        if ((lpMode & ENABLE_VIRTUAL_TERMINAL_PROCESSING) == ENABLE_VIRTUAL_TERMINAL_PROCESSING)
                        {
                            return true;
                        }

                        lpMode |= ENABLE_VIRTUAL_TERMINAL_PROCESSING;

                        if (!SetConsoleMode(handle, lpMode))
                        {
                            return false;
                        }
                        return true;
                    }
                }
                return false;
            }
            return true;

        }

        public static Task ParallelForEachAsync<T>(IEnumerable<T> source, Func<T, Task> funcBody, int maxDoP = 4)
        {
            async Task AwaitPartition(IEnumerator<T> partition)
            {
                using (partition)
                {
                    while (partition.MoveNext())
                    { await funcBody(partition.Current); }
                }
            }

            return Task.WhenAll(
                Partitioner
                    .Create(source)
                    .GetPartitions(maxDoP)
                    .AsParallel()
                    .Select(p => AwaitPartition(p)));
        }

        public static Task ParallelForEachAsync<T1, T2, T3, T4>(IEnumerable<T1> source, Func<T1, T2, T3, T4, Task> funcBody, T2 inputClass, T3 secondInputClass, T4 thirdInputClass, int maxDoP = 4)
        {
            async Task AwaitPartition(IEnumerator<T1> partition)
            {
                using (partition)
                {
                    while (partition.MoveNext())
                    { await funcBody(partition.Current, inputClass, secondInputClass, thirdInputClass); }
                }
            }

            return Task.WhenAll(
                Partitioner
                    .Create(source)
                    .GetPartitions(maxDoP)
                    .AsParallel()
                    .Select(p => AwaitPartition(p)));
        }
    }

    public static class StreamUtil
    {
        public static Task DoubleBufferCopyToAsync(this Stream source, Stream destination, Action<long, int> onProgress = null) 
            => DoubleBufferCopyToAsync(source, destination, 10000, onProgress);

        public static async Task DoubleBufferCopyToAsync(this Stream source, Stream destination, int bufferSize, Action<long, int> onProgress = null)
        {
            byte[] buffer = new byte[bufferSize];
            byte[] secondbuffer = new byte[bufferSize];

            Task writeTask = Task.CompletedTask;
            Task<int> readTask = source.ReadAsync(secondbuffer, 0, bufferSize);

            int size;
            while (true)
            {
                size = await readTask;
                await writeTask;
                onProgress?.Invoke(destination.Position, size);
                if (size <= 0) break;

                readTask = source.ReadAsync(buffer, 0, bufferSize);
                writeTask = destination.WriteAsync(secondbuffer, 0, size);

                size = await readTask;
                await writeTask;
                onProgress?.Invoke(destination.Position, size);
                if (size <= 0) break;

                readTask = source.ReadAsync(secondbuffer, 0, bufferSize);
                writeTask = destination.WriteAsync(buffer, 0, size);
            }
        }

        public static Task CopyToAsync(this Stream source, Stream destination, Action<long, int> onProgress = null)
            => DoubleBufferCopyToAsync(source, destination, 10000, onProgress);

        public static async Task CopyToAsync(this Stream source, Stream destination, int bufferSize, Action<long, int> onProgress = null)
        {
            byte[] buffer = new byte[bufferSize];
            byte[] secondbuffer = new byte[bufferSize];

            int size;
            while ((size = await source.ReadAsync(buffer, 0, bufferSize)) > 0)
            {
                var write = destination.WriteAsync(buffer, 0, size);
                await write;
                onProgress?.Invoke(destination.Position, size);
            }
        }
    }
}
