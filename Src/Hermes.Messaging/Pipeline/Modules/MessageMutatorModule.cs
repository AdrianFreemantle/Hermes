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

        public bool Process(OutgoingMessageContext input, Func<bool> next)
        {            
            MutateMessage(input);
            return next();
        }

        public bool Process(IncomingMessageContext input, Func<bool> next)
        {
            if (input.IsLocalMessage)
                return next();

            MutateMessage(input);
            return next();
        }

        private void MutateMessage(IncomingMessageContext input)
        {
            foreach (var mutator in messageMutators)
            {
                Logger.Debug("{0} is Mutating message body on incomming message {1}", mutator.GetType().Name, input);
                mutator.Mutate(input.Message);
            }
        }

        private void MutateMessage(OutgoingMessageContext output)
        {
            foreach (var mutator in messageMutators)
            {
                Logger.Debug("{0} is Mutating message body on outgoing message {1}", mutator.GetType().Name, output);
                mutator.Mutate(output.OutgoingMessage);
            }
        }
    }
}