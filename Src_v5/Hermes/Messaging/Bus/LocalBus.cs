using System;
using Hermes.Configuration;
using Hermes.Ioc;
using Hermes.Messaging.Transports;

namespace Hermes.Messaging.Bus
{
    public class LocalBus : ILocalBus
    {
        private readonly Transport transport;

        public LocalBus(Transport transport)
        {
            this.transport = transport;
        }

        public void Execute(IDomainCommand command)
        {
            Mandate.ParameterNotNull(command, "command");

            if (!transport.CurrentMessage.Equals(MessageContext.Null))
                throw new InvalidOperationException("A command may not be executed while another command is being processed.");

            var commandContext = new MessageContext
            {
                Message = command,
                Destination = Address.Local,
                ReplyToAddress = Address.Local,
                UserName = RuntimeEnvironment.GetCurrentUserName()
            };

            transport.HandleIncommingMessage(commandContext);
        }

        public void Raise(IDomainEvent @event)
        {
            if (!ServiceLocator.Current.HasServiceProvider())
                throw new InvalidOperationException("A local event may only be raised within the context of an executing local command or received message.");

            var eventContext = new MessageContext
            {
                Message = @event,
                Destination = Address.Local,
                ReplyToAddress = Address.Local,
                UserName = RuntimeEnvironment.GetCurrentUserName(),
            };

            transport.HandleIncommingMessage(eventContext);
        }
    }
}
