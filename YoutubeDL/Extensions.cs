using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace YoutubeDL
{
    public static class ProcessExtensions
    {
        public static Task<int> WaitForExitAsync(this Process process, CancellationToken cancellationToken = default)
        {
            TaskCompletionSource<int> tcs = new TaskCompletionSource<int>();
            if (cancellationToken != default)
            {
                cancellationToken.Register(() =>
                {
                    try
                    {
                        // Send "q" to ffmpeg, which will force it to stop (closing files).
                        process.StandardInput.Write("q");
                    }
                    catch (InvalidOperationException)
                    {
                        // If the process doesn't exist anymore, ignore it.
                    }
                    finally
                    {
                        // Cancel the task. This will throw an exception to the calling program.
                        // Exc.Message will be "A task was canceled."
                        try
                        {
                            tcs.SetCanceled();
                        }
                        catch (Exception)
                        {
                        }
                    }
                });
            }

            process.EnableRaisingEvents = true;
            process.Exited += (sender, e) =>
            {
                process.WaitForExit();
                tcs.TrySetResult(process.ExitCode);
            };

            //var started = process.Start();
            //if (!started)
            //    tcs.TrySetException(new InvalidOperationException($"Could not start process {process}"));

            //process.BeginErrorReadLine();

            return tcs.Task;
        }
    }

#if NETSTANDARD2_0
    public static class DictionaryExtension
    {
        public static TValue GetValueOrDefault<TKey, TValue>
            (   this IDictionary<TKey, TValue> dictionary,TKey key)
        {
            TValue value;
            return dictionary.TryGetValue(key, out value) ? value : default(TValue);
        }
    }
#endif
}
