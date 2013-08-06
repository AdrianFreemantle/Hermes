using System;

namespace Hermes.Backoff
{
    public class ExponentialBackOffStrategy : IBackOffStrategy
    {
        public TimeSpan NextDelay(TimeSpan currentDelay, TimeSpan initialDelay)
        {
            return new TimeSpan(currentDelay.Ticks * 2);
        }
    }
}