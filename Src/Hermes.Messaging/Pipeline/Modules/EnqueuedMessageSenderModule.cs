using System;
using Hermes.Pipes;

namespace Hermes.Messaging.Pipeline.Modules
{
    public class EnqueuedMessageSenderModule : IModule<IncomingMessageContext>
    {
        public bool Process(IncomingMessageContext input, Func<bool> next)
        {
            if (next())
            {
                input.SendOutgoingMessages();
                return true;
            }

            return false;
        }
    }
}