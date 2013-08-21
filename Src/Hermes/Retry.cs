using System;
using System.Threading;

namespace Hermes
{
    public static class Retry
    {
        public static void Action(Action action, int retryAttempts, TimeSpan retryDelay)
        {
            Action(action, (arg1) => { }, retryAttempts, retryDelay);
        }

        public static void Action(Action action, Action<Exception> onRetry, int retryAttempts, TimeSpan retryDelay)
        {
            Mandate.ParameterNotNull(action, "action");

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
                    Thread.Sleep(retryDelay);
                }
            } while (retryCount-- > 0);
        }
    }
}
