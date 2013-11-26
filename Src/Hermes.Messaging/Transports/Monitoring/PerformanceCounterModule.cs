using System;
using System.Timers;

namespace Hermes.Messaging.Transports.Monitoring
{
    public delegate void PerformanceCounterEventHandler(object sender, ProcessingPerformanceCounterEventArgs e);

    public class ProcessingPerformanceCounterEventArgs : EventArgs
    {
        public TransportMessage TransportMessage { get; protected set; }
        public TimeSpan TransitTime { get; protected set; }
        public TimeSpan ProcessingTime { get; protected set; }

        public ProcessingPerformanceCounterEventArgs(TransportMessage transportMessage, TimeSpan transitTime, TimeSpan processingTime)
        {
            TransportMessage = transportMessage;
            TransitTime = transitTime;
            ProcessingTime = processingTime;
        }
    }

    public class PerformanceCounterModule : IModule
    {
        public event PerformanceCounterEventHandler OnPerformanceCalculated;

        public PerformanceCounterModule(ITransportMessages messageTransport)
        {
            messageTransport.OnMessageProcessingCompleted += OnMessageProcessingCompleted;
        }

        void OnMessageProcessingCompleted(object sender, MessageProcessingEventArgs e)
        {
            var completedTime = DateTime.UtcNow;
            var sentTime = e.TransportMessage.Headers[HeaderKeys.SentTime].ToUtcDateTime();
            var receivedTime = e.TransportMessage.Headers[HeaderKeys.ReceivedTime].ToUtcDateTime();

            TimeSpan transitTime = DateTime.UtcNow - sentTime;
            TimeSpan processingTime = completedTime - receivedTime;

            if (OnPerformanceCalculated != null)
            {
                OnPerformanceCalculated(this, new ProcessingPerformanceCounterEventArgs(e.TransportMessage, transitTime, processingTime));
            }
        }
    }
}