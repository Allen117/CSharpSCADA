using System;

namespace Scada.Core.Logging
{
    public class Logger
    {
        private readonly ILogSink _sink;

        public Logger(ILogSink sink)
        {
            _sink = sink;
        }

        public void Info(string message, string coordinatorId = null, string deviceId = null, string pointId = null, string source = "C#")
            => Write(LogLevel.Info, message, coordinatorId, deviceId, pointId, null, source);

        public void Warn(string message, string coordinatorId = null, string deviceId = null, string pointId = null, string source = "C#")
            => Write(LogLevel.Warn, message, coordinatorId, deviceId, pointId, null, source);

        public void Error(string message, Exception ex = null, string coordinatorId = null, string deviceId = null, string pointId = null, string source = "C#")
            => Write(LogLevel.Error, message, coordinatorId, deviceId, pointId, ex, source);

        public void Write(LogLevel level, string message, string coordinatorId, string deviceId, string pointId, Exception ex, string source)
        {
            _sink.Write(new LogEntry
            {
                Level = level,
                Message = message,
                CoordinatorId = coordinatorId,
                DeviceId = deviceId,
                PointId = pointId,
                Exception = ex,
                Source = source
            });
        }
    }
}
