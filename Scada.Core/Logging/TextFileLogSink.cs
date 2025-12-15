using System;
using System.IO;

namespace Scada.Core.Logging
{
    public class TextFileLogSink : ILogSink
    {
        private readonly object _lock = new object();
        private readonly string _filePath;

        public TextFileLogSink(string filePath)
        {
            _filePath = filePath;
        }

        public void Write(LogEntry entry)
        {
            string line = entry.ToString() + Environment.NewLine;

            lock (_lock)
            {
                // 直接 append，客戶用記事本就能看
                File.AppendAllText(_filePath, line);
            }
        }
    }
}
