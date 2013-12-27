using System;
using System.Collections.Generic;
using System.Linq;

using Hermes.Logging;
using Hermes.Pipes;

namespace Hermes.Messaging.Pipeline.Modules
{
    public class MessageMutatorModule : IModule<IncomingMessageContext>, IModule<OutgoingMessageContext>
    {
        private static readonly ILog Logger = LogFactory.BuildLogger(typeof(MessageMutatorModule));

        private readonly IMutateIncomingMessages[] messageMutators;

        public MessageMutatorModule(IEnumerable<IMutateIncomingMessages> messageMutators)
        {
            this.messageMutators = messageMutators.ToArray();
        }

        public bool Invoke(OutgoingMessageContext input, Func<bool> next)
        {
            Logger.Debug("Mutating message body in message {0}", input);
            MutateMessages(input.OutgoingMessages);
            return next();
        }

        public bool Invoke(IncomingMessageContext input, Func<bool> next)
        {
            MutateMessages(input.Messages);
            return next();
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