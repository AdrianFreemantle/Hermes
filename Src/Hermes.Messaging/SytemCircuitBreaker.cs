using System;
using Hermes.Failover;
using Hermes.Messaging.Configuration;

namespace Hermes.Messaging
{
    public static class SytemCircuitBreaker
    {
        private static readonly CircuitBreaker CircuitBreaker;

        static SytemCircuitBreaker()
        {
            CircuitBreaker = new CircuitBreaker(Settings.CircuitBreakerThreshold, Settings.CircuitBreakerReset);
        }

        public static void Execute(Action trigger)
        {
            CircuitBreaker.Execute(trigger);
        }
    }
}