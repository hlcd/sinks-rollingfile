using System;
using System.IO;
using System.Text.RegularExpressions;

namespace Serilog.Sinks.RollingFileAlternate.Sinks.SizeRollingFileSink
{
    internal class LogFileInfo
    {
        private const string NumberFormat = "00000";
        private const string DateFormat = "yyyyMMdd";

        internal uint Sequence { get; private set; }
        internal string FileName { get; private set; }
        internal DateTime Date { get; private set; }
        internal string Prefix { get; private set; }

        public LogFileInfo(DateTime date, uint sequence, string prefix = null)
        {
            this.Date = date;
            this.Sequence = sequence;
            this.Prefix = prefix;
            this.FileName = String.Format("{2}{0}-{1}.log", date.ToString(DateFormat), sequence.ToString(NumberFormat), Prefix != null ? Prefix + "-" : "");
        }

        public LogFileInfo Next()
        {
            DateTime now = DateTime.UtcNow;
            if (this.Date.Date != now.Date)
            {
                return new LogFileInfo(now, 1, Prefix);
            }

            return new LogFileInfo(now, this.Sequence + 1, Prefix);
        }

        internal static LogFileInfo GetLatestOrNew(DateTime date, string logDirectory, string prefix = null)
        {
            string pattern = date.ToString(DateFormat) + @"-(\d{5}).log";

            var logFileInfo = new LogFileInfo(date, 1, prefix);

            foreach (var filePath in Directory.GetFiles(logDirectory))
            {
                Match match = Regex.Match(filePath, pattern);
                if (match.Success)
                {
                    var sequence = uint.Parse(match.Groups[1].Value);

                    if (sequence > logFileInfo.Sequence)
                    {
                        logFileInfo = new LogFileInfo(date, sequence, prefix);
                    }
                }
            }

            return logFileInfo;
        }
    }
}
