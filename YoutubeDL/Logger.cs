using System;
using System.Collections.Generic;

namespace YoutubeDL
{
    public interface IHasLog
    {
        public event LogEventHandler OnLog;
    }

    public enum LogType
    {
        Debug,
        Info,
        Warning,
        Error,
    }

    public delegate void LogEventHandler(object sender, LogEventArgs message);

    public class LogEventArgs : EventArgs
    {
        public string Sender { get; set; }
        public string RawMessage { get; set; }
        public string Message { get; set; }
        public string ColoredMessage { get; set; }
        public LogType LogType { get; set; }
    }

    internal class Logger
    {
        private static Logger logger;
        public static Logger Instance
        {
            get
            {
                if (logger == null) logger = new Logger();
                return logger;
            }
        }

        public LogEventArgs Log(string message, LogType type, string[] sender = null, bool writeline = true, string colormessage = null, bool ytdlpy = false)
        {
            LogEventArgs args = new LogEventArgs
            {
                LogType = type,
                RawMessage = message
            };

            if (ytdlpy)
            {
                List<string> senders = sender == null ? new List<string>() : new List<string>(sender);
                while (message.StartsWith('['))
                {
                    int bracketindex = message.IndexOf(']');
                    senders.Add(message.Substring(1, bracketindex - 1));
                    //colormessage = "\u001b[92m->\u001b[96m[\u001b[95m" + message.Substring(1, bracketindex - 1) + "\u001b[96m]\u001b[0m" + message.Substring(bracketindex + 1);
                    message = message.Substring(bracketindex + 2);
                }
                sender = senders.ToArray();
            }
            else if (sender == null)
                sender = new string[] { };

            if (colormessage == null)
                colormessage = message;

            string prefix = ytdlpy ? "youtube-dl-python" : "youtube-dl";
            string colorprexix = "\u001b[96m[\u001b[95m" + prefix + "\u001b[96m]\u001b[0m";
            prefix = "[" + prefix + "]";

            args.Message = prefix;
            args.ColoredMessage = colorprexix;

            foreach (string sen in sender)
            {
                args.Message += $"->[{sen}]";
                args.ColoredMessage += $"\u001b[92m->\u001b[96m[\u001b[95m{sen}\u001b[96m]\u001b[0m";
            }

            if (type == LogType.Warning)
            {
                args.Message += $" WARNING: {message}";
                args.ColoredMessage += $" \u001b[33mWARNING: {colormessage}\u001b[0m";
            }
            else if (type == LogType.Error)
            {
                args.Message += $" ERROR: {message}";
                args.ColoredMessage += $" \u001b[31mERROR: {colormessage}\u001b[0m";
            }
            else
            {
                args.Message += $" {message}";
                args.ColoredMessage += $" {colormessage}\u001b[0m";
            }

            if (writeline)
            {
                args.Message += "\n";
                args.ColoredMessage += "\n";
            }

            return args;
        }
    }
}
