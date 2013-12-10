using System;
using System.Collections.Generic;
using System.Linq;
using Hermes.Pipes;

namespace Hermes.Messaging.Pipeline.Modules
{
    public class MessageMutatorModule : IModule<IncomingMessageContext>, IModule<OutgoingMessageContext>
    {
        private readonly IMutateIncomingMessages[] messageMutators;

        public MessageMutatorModule(IEnumerable<IMutateIncomingMessages> messageMutators)
        {
            this.messageMutators = messageMutators.ToArray();
        }

        public void Invoke(OutgoingMessageContext input, Action next)
        {
            MutateMessages(input.OutgoingMessages);
            next();
        }

        public void Invoke(IncomingMessageContext input, Action next)
        {
            MutateMessages(input.Messages);
            next();
        }

        private void MutateMessages(IEnumerable<object> messages)
        {
            foreach (var message in messages)
            {
                MutateMessage(message);
            }
        }

        private void MutateMessage(object message)
        {
            foreach (var mutator in messageMutators)
            {
                mutator.Mutate(message);
            }
        }        
    }
}