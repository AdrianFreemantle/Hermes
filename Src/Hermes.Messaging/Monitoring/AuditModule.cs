using System;

using Hermes.Messaging.Configuration;
using Hermes.Messaging.Transports;

namespace Hermes.Messaging.Monitoring
{
    public class AuditModule : IModule
    {
        private readonly ISendMessages messageSender;

        public AuditModule(ITransportMessages messageTransport, ISendMessages messageSender)
        {
            this.messageSender = messageSender;

            if (!Settings.IsSendOnly)
            {
                messageTransport.OnMessageProcessingCompleted += OnMessageProcessingCompleted;
                messageTransport.OnMessageReceived += OnOnMessageReceived;
            }
        }

        private void OnOnMessageReceived(object sender, MessageProcessingEventArgs e)
        {
            e.TransportMessage.Headers[HeaderKeys.ReceivedTime] = DateTime.UtcNow.ToWireFormattedString();
        }

        void OnMessageProcessingCompleted(object sender, MessageProcessingEventArgs e)
        {
            try
            {
                RemoveRetryHeaders(e.TransportMessage);
                e.TransportMessage.Headers[HeaderKeys.CompletedTime] = DateTime.UtcNow.ToWireFormattedString();
                e.TransportMessage.Headers[HeaderKeys.ProcessingEndpoint] = Address.Local.ToString();
                messageSender.Send(e.TransportMessage, Settings.AuditEndpoint);
            }
            catch
            {
                e.TransportMessage.Headers.Remove(HeaderKeys.CompletedTime);
                throw;
            }
        }

        public void RemoveRetryHeaders(TransportMessage envelope)
        {
            envelope.Headers.Remove(HeaderKeys.TimeoutExpire);
            envelope.Headers.Remove(HeaderKeys.RouteExpiredTimeoutTo);
            envelope.Headers.Remove(HeaderKeys.FailureDetails);
        }
    }
}