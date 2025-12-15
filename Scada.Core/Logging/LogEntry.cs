using System;

namespace Scada.Core.Logging
{
    public class LogEntry
    {
        public DateTime Time { get; set; } = DateTime.Now;
        public LogLevel Level { get; set; } = LogLevel.Info;

        // 方便你未來做 VB vs C# 比對
        public string Source { get; set; } = "C#";

        // 你的 SCADA 常用欄位
        public string CoordinatorId { get; set; }
        public string DeviceId { get; set; }
        public string PointId { get; set; }

        public string Message { get; set; }
        public Exception Exception { get; set; }

        public override string ToString()
        {
            // 你原本習慣：時間 + 內容
            // 我幫你加上固定欄位，仍然是純文字好讀
            string ex = Exception == null ? "" : (" | EX=" + Exception.GetType().Name + ":" + Exception.Message);

            return $"{Time:yyyy-MM-dd HH:mm:ss.fff} | LV={Level} | SRC={Source}" +
                   $" | CO={CoordinatorId} | DEV={DeviceId} | PT={PointId}" +
                   $" | MSG={Message}{ex}";
        }
    }
}
