using System;
using System.Collections.Generic;
using System.Linq;
using Hermes.Logging;
using Hermes.Pipes;

namespace Hermes.Messaging.Transports.Modules
{
    public class MessageMutatorModule : IModule<IncomingMessageContext>
    {
        private readonly IMutateIncomingMessages[] messageMutators;

        public MessageMutatorModule(IEnumerable<IMutateIncomingMessages> messageMutators)
        {
            this.messageMutators = messageMutators.ToArray();
        }

        public void Invoke(IncomingMessageContext input, Action next)
        {
            foreach (var message in input.Messages)
            {
                MutateMessage(message);
            }

            next();
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