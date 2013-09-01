using System;
using System.Collections.Generic;

namespace Hermes.Messaging
{
    public interface IProcessMessages
    {
        void ProcessEnvelope(TransportMessage transportMessage);
        void ProcessMessages(IEnumerable<object> messageBodies);

        event EventHandler<StartedMessageProcessingEventArgs> StartedMessageProcessing;
        event EventHandler<CompletedMessageProcessingEventArgs> CompletedMessageProcessing;
        event EventHandler<FailedMessageProcessingEventArgs> FailedMessageProcessing;
    }
}
