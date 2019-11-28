using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace YoutubeDL
{
    public interface IHasProgress
    {
        public event ProgressEventHandler OnProgress;
    }

    public class ProgressEventArgs : EventArgs
    {
        public ProgressEventArgs(long value, long total, string unit, DateTime startTime)
        {
            TimePast = DateTime.Now - startTime;
            StartTime = startTime;
            Value = value;
            Total = total;
            Unit = unit;
            HasTotal = total > 0;
            //Debug.WriteLine("val: " + value + ", total: " + Total);
            if (HasTotal)
            {
                TimeRemaining = ProgressUtil.CalcRemainingTime(TimePast, value, total);
                Percent = ProgressUtil.CalcPercent(value, total);
                PercentRatio = ProgressUtil.CalcPercentRatio(value, total);
            }
            Speed = ProgressUtil.CalcSpeed(TimePast, value);
            SpeedString = ProgressUtil.GetSuffix(Speed) + unit + "ps";
        }

        public long Value { get; protected set; }
        public long Total { get; protected set; }
        public DateTime StartTime { get; protected set; }
        public TimeSpan TimePast { get; protected set; }
        public TimeSpan? TimeRemaining { get; protected set; }
        public double Percent { get; protected set; }
        public double PercentRatio { get; protected set; }
        public double Speed { get; protected set; }
        public string SpeedString { get; protected set; }
        public string Unit { get; protected set; }
        public bool HasTotal { get; protected set; }
    }

    public delegate void ProgressEventHandler(object sender, ProgressEventArgs e);

    public static class ProgressUtil
    {
        private static List<Tuple<int, string>> ZeroesAndLetters = new List<Tuple<int, string>>()
        {
            new Tuple<int, string>(18, "E"),
            new Tuple<int, string>(15, "P"),
            new Tuple<int, string>(12, "T"),
            new Tuple<int, string>(9, "G"),
            new Tuple<int, string>(6, "M"),
            new Tuple<int, string>(3, "K"),
        };

        public static string GetSuffix(double num)
        {
            int zeroCount = ((long)num).ToString().Length;
            for (int i = 0; i < ZeroesAndLetters.Count; i++)
                if (zeroCount >= ZeroesAndLetters[i].Item1)
                    return Math.Round(num / Math.Pow(10, ZeroesAndLetters[i].Item1), 2).ToString() + " " + ZeroesAndLetters[i].Item2;
            return num.ToString();
        }
        public static double CalcSpeed(TimeSpan time_past, long bytes)
        {
            if (bytes == 0 || time_past.TotalSeconds < 0.001d)
                return 0;
            return bytes / time_past.TotalSeconds;
        }

        public static double CalcSpeed(DateTime start, DateTime current, long bytes)
        {
            TimeSpan time_past = current - start;
            return CalcSpeed(time_past, bytes);
        }

        public static double CalcPercent(long value, long total) => CalcPercentRatio(value, total) * 100f;
        public static double CalcPercentRatio(long value, long total) => (float)value / total;

        public static TimeSpan? CalcRemainingTime(TimeSpan time_past, long value, long total)
        {
            double speed = CalcSpeed(time_past, value);
            if (speed == 0d) return null;
            return TimeSpan.FromSeconds((total - value) / CalcSpeed(time_past, value));
        }
    }
}
