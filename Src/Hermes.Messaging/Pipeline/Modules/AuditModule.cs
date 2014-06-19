using System;
using Hermes.Failover;
using Hermes.Logging;
using Hermes.Messaging.Configuration;
using Hermes.Messaging.Transports;
using Hermes.Pipes;

namespace Hermes.Messaging.Pipeline.Modules
{
    public class AuditModule : IModule<IncomingMessageContext>
    {
        private readonly static ILog Logger = LogFactory.BuildLogger(typeof(AuditModule));

        private readonly ISendMessages messageSender;

        public AuditModule(ISendMessages messageSender)
        {
            this.messageSender = messageSender;
        }

        public bool Process(IncomingMessageContext input, Func<bool> next)
        {
            DateTime receivedTime = DateTime.UtcNow;

            Logger.Info("Starting processing chain for message {0}", input);

            if (next())
            {
                SendToAuditQueue(input.TransportMessage, receivedTime);
                Logger.Info("Completed processing chain for message {0}", input);
                FaultSimulator.Trigger();
                return true;
            }

            return false;
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