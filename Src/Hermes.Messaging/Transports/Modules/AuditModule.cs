using System;
using Hermes.Logging;
using Hermes.Messaging.Configuration;
using Hermes.Pipes;

namespace Hermes.Messaging.Transports.Modules
{
    public class AuditModule : IModule<IncomingMessageContext>
    {
        private readonly ISendMessages messageSender;

        public AuditModule(ISendMessages messageSender)
        {
            this.messageSender = messageSender;
        }

        public void Invoke(IncomingMessageContext input, Action next)
        {
            DateTime receivedTime = DateTime.UtcNow;
            next();
            SendToAuditQueue(input.TransportMessage, receivedTime);
        }

        private void SendToAuditQueue(TransportMessage transportMessage, DateTime receivedTime)
        {
            try
            {
                ProcessCompletedHeaders(transportMessage, receivedTime);
                messageSender.Send(transportMessage, Settings.AuditEndpoint);
            }
            catch
            {
                RemoveCompletedHeaders(transportMessage);
                throw;
            }
        }

        private static void RemoveCompletedHeaders(TransportMessage transportMessage)
        {
            transportMessage.Headers.Remove(HeaderKeys.CompletedTime);
            transportMessage.Headers.Remove(HeaderKeys.ReceivedTime);
            transportMessage.Headers.Remove(HeaderKeys.ProcessingEndpoint);
        }

        private void ProcessCompletedHeaders(TransportMessage transportMessage, DateTime receivedTime)
        {
            transportMessage.Headers.Remove(HeaderKeys.TimeoutExpire);
            transportMessage.Headers.Remove(HeaderKeys.RouteExpiredTimeoutTo);
            transportMessage.Headers.Remove(HeaderKeys.FailureDetails);

            transportMessage.Headers[HeaderKeys.ProcessingEndpoint] = Address.Local.ToString();
            transportMessage.Headers[HeaderKeys.ReceivedTime] = receivedTime.ToWireFormattedString();
            transportMessage.Headers[HeaderKeys.CompletedTime] = DateTime.UtcNow.ToWireFormattedString();
        }
    }
}