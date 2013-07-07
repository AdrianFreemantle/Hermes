using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Hermes.Backoff
{
    public class BackOff
    {
        private readonly TimeSpan maximumDelayTime = TimeSpan.FromSeconds(30);
        private readonly TimeSpan intialDelayTime = TimeSpan.FromSeconds(1);
        private readonly IBackOffStrategy backOffStrategy;
        private TimeSpan currentDelayTime = TimeSpan.FromSeconds(1);

        public BackOff()
        {
            backOffStrategy = new ExponentialBackOffStrategy();
        }

        public BackOff(TimeSpan intialDelayTime, TimeSpan maximumDelayTime)
        {
            this.intialDelayTime = intialDelayTime;
            this.maximumDelayTime = maximumDelayTime;
        }

        public BackOff(IBackOffStrategy backOffStrategy, TimeSpan intialDelayTime, TimeSpan maximumDelayTime)
        {
            this.backOffStrategy = backOffStrategy;
            this.intialDelayTime = intialDelayTime;
            this.maximumDelayTime = maximumDelayTime;
        }

        public BackOff(IBackOffStrategy backOffStrategy)
        {
            this.backOffStrategy = backOffStrategy;
        }

        public void Delay()
        {
            Thread.Sleep(currentDelayTime);
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
