using System;
using System.Threading;

namespace Hermes.Backoff
{
    public class BackOff
    {
        private readonly TimeSpan maximumDelayTime;
        private readonly TimeSpan intialDelayTime;
        private readonly IBackOffStrategy backOffStrategy;
        private TimeSpan currentDelayTime;

        public BackOff()
            : this(new ExponentialBackOffStrategy(), TimeSpan.FromMilliseconds(1), TimeSpan.FromSeconds(30))
        {
        }

        public BackOff(TimeSpan intialDelayTime, TimeSpan maximumDelayTime)
            : this(new ExponentialBackOffStrategy(), intialDelayTime, maximumDelayTime)
        {
        }

        public BackOff(IBackOffStrategy backOffStrategy, TimeSpan intialDelayTime, TimeSpan maximumDelayTime)
        {
            this.backOffStrategy = backOffStrategy;
            this.intialDelayTime = intialDelayTime;
            this.maximumDelayTime = maximumDelayTime;
            currentDelayTime = intialDelayTime;
        }      

        public void Delay()
        {
            Thread.Sleep(currentDelayTime);
            UpdateDelayTime();
        }

        public void Delay(CancellationToken token)
        {
            bool cancelled = token.WaitHandle.WaitOne(currentDelayTime);

            if(cancelled)
                return;

            UpdateDelayTime();
        }

        private void UpdateDelayTime()
        {
            var newDelayTime = backOffStrategy.NextDelay(currentDelayTime, intialDelayTime);
            currentDelayTime = newDelayTime > maximumDelayTime ? maximumDelayTime : newDelayTime;
        }

        public void Reset()
        {
            currentDelayTime = intialDelayTime;
        }
    }
}
