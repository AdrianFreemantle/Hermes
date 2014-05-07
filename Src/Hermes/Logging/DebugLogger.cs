using System;

namespace Hermes.Logging
{
    public class DebugLogger : ILog
    {
        private readonly Type typeToLog;

        public static LogLevel MinimumLogLevel { get; set; }

        public DebugLogger(Type typeToLog)
        {
            this.typeToLog = typeToLog;
        }

        public virtual void Verbose(string message, params object[] values)
        {
            if (MinimumLogLevel <= LogLevel.Verbose)
                Log(message, values);
        }

        public virtual void Debug(string message, params object[] values)
        {
            if (MinimumLogLevel <= LogLevel.Debug)
                Log(message, values);
        }

        public virtual void Info(string message, params object[] values)
        {
            if (MinimumLogLevel <= LogLevel.Info)
                Log(message, values);
        }

        public virtual void Warn(string message, params object[] values)
        {
            if (MinimumLogLevel <= LogLevel.Warn)
                Log(message, values);
        }

        public virtual void Error(string message, params object[] values)
        {
            if (MinimumLogLevel <= LogLevel.Error)
                Log(message, values);
        }

        public virtual void Fatal(string message, params object[] values)
        {
            if (MinimumLogLevel <= LogLevel.Fatal)
                Log(message, values);
        }

        private void Log(string message, params object[] values)
        {
            System.Diagnostics.Debug.WriteLine(message.FormatMessage(typeToLog, values));
        }
    }
}