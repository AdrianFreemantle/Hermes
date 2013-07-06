using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Hermes
{
    public static class Retry
    {
        public static void Action(Action action, int retryAttempts, int retryMilliseconds)
        {
            Action(action, (ex) => { }, retryAttempts, retryMilliseconds);
        }

        public static void Action(Action action, Action<Exception> onError, int retryAttempts, int retryMilliseconds)
        {
            Mandate.ParameterNotNull(action, "action");

            do
            {
                try
                {
                    action();
                    return;
                }
                catch (Exception ex)
                {
                    onError(ex);

                    if (retryAttempts <= 0)
                    {
                        throw;
                    }

                    Thread.Sleep(retryMilliseconds);
                }
            } while (retryAttempts-- > 0);
        }
    }
}
