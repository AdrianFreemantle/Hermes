using System;

namespace Hermes.Messaging
{
    public interface IProcessIncomingMessages
    {
        void ProcessTransportMessage(TransportMessage transportMessage);

        //event EventHandler<StartedMessageProcessingEventArgs> StartedMessageProcessing;
        //event EventHandler<CompletedMessageProcessingEventArgs> CompletedMessageProcessing;
        //event EventHandler<FailedMessageProcessingEventArgs> FailedMessageProcessing;
    }
}
