using System;

namespace Hermes.Logging
{
    public enum LogLevel
    {
        Debug,
        Info,
        Warn,
        Error,
        Fatal,
        NoLogging
    }

    public class ConsoleWindowLogger : ILog
    {
        private static readonly object Sync = new object();
        private readonly ConsoleColor originalColor = Console.ForegroundColor;
        private readonly Type typeToLog;

        public static LogLevel MinimumLogLevel { get; set; }        

        public ConsoleWindowLogger(Type typeToLog)
        {
            this.typeToLog = typeToLog;
        }

        public virtual void Debug(string message, params object[] values)
        {
            if (MinimumLogLevel <= LogLevel.Debug)
                Log(ConsoleColor.Green, message, values);
        }

        public virtual void Info(string message, params object[] values)
        {
            if (MinimumLogLevel <= LogLevel.Info)
                Log(ConsoleColor.DarkCyan, message, values);
        }

        public virtual void Warn(string message, params object[] values)
        {
            if (MinimumLogLevel <= LogLevel.Warn)
                Log(ConsoleColor.Yellow, message, values);
        }

        public virtual void Error(string message, params object[] values)
        {
            if (MinimumLogLevel <= LogLevel.Error)
                Log(ConsoleColor.DarkRed, message, values);
        }

        public virtual void Fatal(string message, params object[] values)
        {
            if (MinimumLogLevel <= LogLevel.Fatal)
                Log(ConsoleColor.Red, message, values);
        }

        private void Log(ConsoleColor color, string message, params object[] values)
        {
            lock (Sync)
            {
                Console.ForegroundColor = color;
                Console.WriteLine(message.FormatMessage(this.typeToLog, values));
                Console.ForegroundColor = this.originalColor;
            }
        }
    }
}
