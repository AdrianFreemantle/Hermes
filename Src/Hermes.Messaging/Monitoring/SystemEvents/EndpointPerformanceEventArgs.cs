using System;

namespace Hermes.Messaging.Monitoring.SystemEvents
{
    public class EndpointPerformanceEventArgs : EventArgs
    {
        public Address Endpoint { get; protected set; }
        public int TotalMessagesProcessed { get; protected set; }
        public int TotalErrorMessages { get; protected set; }
        public TimeSpan AverageTimeToDeliver { get; protected set; }
        public TimeSpan AverageTimeToProcess { get; protected set; }
        public TimeSpan MonitoringPeriod { get; protected set; }

        public EndpointPerformanceEventArgs(Address endpoint, int totalMessagesProcessed, int totalErrorMessages, TimeSpan averageTimeToDeliver, TimeSpan averageTimeToProcess, TimeSpan monitoringPeriod)
        {
            Endpoint = endpoint;
            TotalMessagesProcessed = totalMessagesProcessed;
            TotalErrorMessages = totalErrorMessages;
            AverageTimeToDeliver = averageTimeToDeliver;
            AverageTimeToProcess = averageTimeToProcess;
            MonitoringPeriod = monitoringPeriod;
        }
    }
}