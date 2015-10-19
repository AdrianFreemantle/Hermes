using System;
using Hermes.Logging;

namespace Hermes.Failover
{
    public static class SystemCircuitBreaker
    {
        public static CircuitBreaker CircuitBreaker { get; set; }
        private static readonly ILog Logger = LogFactory.BuildLogger(typeof(SystemCircuitBreaker));

        static SystemCircuitBreaker()
        {
            CircuitBreaker = new CircuitBreaker(10, TimeSpan.FromSeconds(30));
        }

        public static void Trigger(Exception ex)
        {
            Logger.Error(ex.GetFullExceptionMessage());

            CircuitBreaker.Execute(() => CriticalError.Raise("Sytem Circuit Breaker Triggered", ex));
        }

        public static void Trigger(Exception ex, string message)
        {
            Logger.Error(String.Format("{0}:\n{1}", message, ex.GetFullExceptionMessage()));
            CircuitBreaker.Execute(() => CriticalError.Raise(message, ex));
        }
    }
}