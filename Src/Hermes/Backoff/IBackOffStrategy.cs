using System;

namespace Hermes.Backoff
{
    public interface IBackOffStrategy
    {
        TimeSpan NextDelay(TimeSpan currentDelay, TimeSpan initialDelay);
    }
}