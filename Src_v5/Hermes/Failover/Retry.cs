using System;
using System.Diagnostics;
using System.Threading;
using Hermes.Backoff;

namespace Hermes.Failover
{
    [DebuggerStepThrough]
    public static class Retry
    {
        public static void Action(Action action, int retryAttempts)
        {
            Action(action, (arg1) => { }, retryAttempts, new BackOff(), CancellationToken.None);
        }

        public static void Action(Action action, int retryAttempts, CancellationToken token)
        {
            Action(action, (arg1) => { }, retryAttempts, new BackOff(), token);
        }

        public static void Action(Action action, Action<Exception> onRetry, int retryAttempts)
        {
            Action(action, onRetry, retryAttempts, new BackOff(), CancellationToken.None);
        }

        public static void Action(Action action, Action<Exception> onRetry, int retryAttempts, CancellationToken token)
        {
            Action(action, onRetry, retryAttempts, new BackOff(), token);
        }

        public static void Action(Action action, int retryAttempts, BackOff backOff)
        {
            Action(action, (arg1) => { }, retryAttempts, backOff, CancellationToken.None);
        }

        public static void Action(Action action, int retryAttempts, BackOff backOff, CancellationToken token)
        {
            Action(action, (arg1) => { }, retryAttempts, backOff, token);
        }

        public static void Action(Action action, Action<Exception> onRetry, int retryAttempts, BackOff backOff)
        {
            Action(action, (arg1) => { }, retryAttempts, backOff, CancellationToken.None);
        }

        public static void Action(Action action, Action<Exception> onRetry, int retryAttempts, BackOff backOff, CancellationToken token)
        {
            Mandate.ParameterNotNull(action, "action");
            Mandate.ParameterNotNull(backOff, "backOff");

            backOff.Reset();

            int retryCount = retryAttempts;

            do
            {
                try
                {
                    action();
                    return;
                }
                catch (Exception ex)
                {
                    if (retryCount <= 0)
                    {
                        throw;
                    }

                    onRetry(ex);
                    backOff.Delay(CancellationToken.None);
                }
            } while (retryCount-- > 0 && !token.IsCancellationRequested);
        }
    }
}
