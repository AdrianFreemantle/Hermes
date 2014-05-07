using System;
using System.Diagnostics;
using Hermes.Backoff;

namespace Hermes.Failover
{
    [DebuggerStepThrough]
    public static class Retry
    {
        public static void Action(Action action, Action<Exception> onRetry, int retryAttempts)
        {
            Action(action, (arg1) => { }, retryAttempts, new BackOff());
        }

        public static void Action(Action action, int retryAttempts, BackOff backOff)
        {
            Action(action, (arg1) => { }, retryAttempts, backOff);
        }

        public static void Action(Action action, Action<Exception> onRetry, int retryAttempts, BackOff backoff)
        {
            Mandate.ParameterNotNull(action, "action");
            Mandate.ParameterNotNull(backoff, "backoff");

            backoff.Reset();

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
                    backoff.Delay();
                }
            } while (retryCount-- > 0);
        }
    }
}
