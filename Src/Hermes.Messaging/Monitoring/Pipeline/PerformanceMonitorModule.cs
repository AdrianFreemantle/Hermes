using System;
using Hermes.Messaging.Monitoring.SystemEvents;
using Hermes.Messaging.Pipeline;
using Hermes.Messaging.Transports;
using Hermes.Pipes;

namespace Hermes.Messaging.Monitoring.Pipeline
{
    public class PerformanceMonitorModule : IModule<IncomingMessageContext>
    {
        public static event EndpointPerformanceMetricReceivedHandler OnEndpointPerformanceMetricReceived;

        public bool Process(IncomingMessageContext input, Func<bool> next)
        {
            HeaderValue totalMessagesProcessed;
            HeaderValue totalErrorMessages;
            HeaderValue averageTimeToDeliver;
            HeaderValue averageTimeToProcess;
            HeaderValue monitoringPeriod;

            input.TryGetHeaderValue(HeaderKeys.TotalMessagesProcessed, out totalMessagesProcessed);
            input.TryGetHeaderValue(HeaderKeys.TotalErrorMessages, out totalErrorMessages);
            input.TryGetHeaderValue(HeaderKeys.AverageTimeToDeliver, out averageTimeToDeliver);
            input.TryGetHeaderValue(HeaderKeys.AverageTimeToProcess, out averageTimeToProcess);
            input.TryGetHeaderValue(HeaderKeys.MonitoringPeriod, out monitoringPeriod);

            if (totalMessagesProcessed != null && OnEndpointPerformanceMetricReceived != null)
            {
                int processed = Int32.Parse(totalMessagesProcessed.Value);
                int errors = Int32.Parse(totalErrorMessages.Value);
                var timeToDeliver = TimeSpan.Parse(averageTimeToDeliver.Value);
                var timeToProcess = TimeSpan.Parse(averageTimeToProcess.Value);
                var period = TimeSpan.FromSeconds(Int32.Parse(monitoringPeriod.Value));

                var eventArgs = new EndpointPerformanceEventArgs(input.ReplyToAddress, processed, errors, timeToDeliver, timeToProcess, period);
                OnEndpointPerformanceMetricReceived(eventArgs, this);

                return true;
            }

            return next();
        }
    }
}
