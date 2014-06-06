using System;
using Hermes.Logging;

namespace Hermes.Failover
{
    public delegate void CriticalErrorHandler(CriticalErrorEventArgs e);

    public class CriticalErrorEventArgs : EventArgs
    {
        public string Message { get; protected set; }
        public Exception Exception { get; protected set; }

        public CriticalErrorEventArgs(string message, Exception exception)
        {
            Message = message;
            Exception = exception;
        }
    }

    public static class CriticalError
    {
        private static readonly ILog Logger = LogFactory.BuildLogger(typeof(CriticalError));

        private static Action<string, Exception> onCriticalErrorAction = (message, exception) => { };

        public static event CriticalErrorHandler OnCriticalError;

        /// <summary>
        /// Sets the function to be used when critical error occurs.
        /// </summary>
        /// <param name="onCriticalError">Assigns the action to perform on critical error.</param>
        /// <returns>The configuration object.</returns>
        public static void DefineCriticalErrorAction(Action<string, Exception> onCriticalError)
        {
            onCriticalErrorAction = onCriticalError;
        }

        /// <summary>
        /// Execute the configured Critical error action.
        /// </summary>
        /// <param name="message">Error message</param>
        /// <param name="exception">The critical exception thrown.</param>
        public static void Raise(string message, Exception exception)
        {
            Logger.Fatal("{0} :\n{1}", message, exception.GetFullExceptionMessage());

            RaiseEvent(message, exception);

            onCriticalErrorAction(message, exception);
        }

        private static void RaiseEvent(string message, Exception exception)
        {
            try
            {
                if (OnCriticalError != null)
                {
                    OnCriticalError(new CriticalErrorEventArgs(message, exception));
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Error while raising OnCriticalError event : {0}", ex.GetFullExceptionMessage());
            }
        }
    }
}
