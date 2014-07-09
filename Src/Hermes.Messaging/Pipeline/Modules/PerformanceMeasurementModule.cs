using System;
using System.Timers;
using Hermes.Messaging.Bus;
using Hermes.Messaging.Configuration;
using Hermes.Messaging.Monitoring;
using Hermes.Messaging.Transports;
using Hermes.Pipes;

namespace Hermes.Messaging.Pipeline.Modules
{
    public class PerformanceMeasurementModule : IModule<IncomingMessageContext>, IDisposable
    {
        private readonly PerformanceMetricCollection performanceMetrics;
        private readonly ControlBus bus;
        private readonly Timer timer;
        private readonly TimeSpan monitoringPeriod = TimeSpan.FromSeconds(10);

        public PerformanceMeasurementModule(ControlBus bus)
        {
            this.bus = bus;
            performanceMetrics = new PerformanceMetricCollection();

            timer = new Timer
            {
                Interval = monitoringPeriod.TotalMilliseconds,
                AutoReset = true,
            };

            timer.Elapsed += Elapsed;

            Start();
        }

        private void Start()
        {
            if (Settings.DisablePerformanceMonitoring)
                return;

            timer.Start();
        }

        public bool Process(IncomingMessageContext input, Func<bool> next)
        {
            if (Settings.DisablePerformanceMonitoring)
            {
                return next();
            }

            var result = next();
            performanceMetrics.Add(input);
            return result;
        }

        void Elapsed(object sender, ElapsedEventArgs e)
        {
            PerformanceMetric endpointPerformance = performanceMetrics.GetEndpointPerformance();
            
            var headers = new[]
            {
                new HeaderValue(HeaderKeys.AverageTimeToProcess, endpointPerformance.AverageTimeToProcess.ToString()),
                new HeaderValue(HeaderKeys.AverageTimeToDeliver, endpointPerformance.AverageTimeToDeliver.ToString()),
                new HeaderValue(HeaderKeys.TotalErrorMessages, endpointPerformance.TotalErrorMessages.ToString()),
                new HeaderValue(HeaderKeys.TotalMessagesProcessed, endpointPerformance.TotalMessagesProcessed.ToString()),
                new HeaderValue(HeaderKeys.MonitoringPeriod, monitoringPeriod.Seconds.ToString()),
            };

            bus.Send(Settings.MonitoringEndpoint, headers);
        }

        public void Dispose()
        {
            timer.Dispose();
        }
    }
}