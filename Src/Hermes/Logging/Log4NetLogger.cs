using System;

namespace Hermes.Logging
{
    public class Log4NetLogger : ILog
    {
        private readonly log4net.ILog log;

        public Log4NetLogger(Type typeToLog)
        {
            log = log4net.LogManager.GetLogger(typeToLog);
        }

        public virtual void Verbose(string message, params object[] values)
        {
            if (log.IsDebugEnabled)
                log.DebugFormat(message, values);
        }
        public virtual void Debug(string message, params object[] values)
        {
            if (log.IsDebugEnabled)
                log.DebugFormat(message, values);
        }
        public virtual void Info(string message, params object[] values)
        {
            if (log.IsInfoEnabled)
                log.InfoFormat(message, values);
        }
        public virtual void Warn(string message, params object[] values)
        {
            if (log.IsWarnEnabled)
                log.WarnFormat(message, values);
        }
        public virtual void Error(string message, params object[] values)
        {
            if (log.IsErrorEnabled)
                log.ErrorFormat(message, values);
        }
        public virtual void Fatal(string message, params object[] values)
        {
            if (log.IsFatalEnabled)
                log.FatalFormat(message, values);
        }
    }
}
