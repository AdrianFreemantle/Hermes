using System;
using Hermes.Logging;
using Hermes.Messaging.Bus;
using Hermes.Messaging.Configuration;
using Hermes.Messaging.Transports;

namespace Hermes.Messaging.Monitoring
{
    public class HeartbeatService : ScheduledWorkerService
    {
        private readonly ControlBus controlBus;

        public HeartbeatService(ControlBus controlBus)
        {
            this.controlBus = controlBus;
            this.SetSchedule(TimeSpan.FromSeconds(30));
        }

        public override void Start()
        {
            if (Settings.DisableHeartbeatService)
                return;

            base.Start();
        }

        protected override void DoWork()
        {
            var headers = new[]
            {
                new HeaderValue(HeaderKeys.Heartbeat, Address.Local.ToString()),
            };

            controlBus.Send(Settings.MonitoringEndpoint, headers);
        }
    }
}