using System;
using Hermes.Configuration;
using Hermes.Ioc;
using Hermes.Logging;
using Hermes.Messaging.Transports;

namespace Hermes.Messaging.Bus
{
    public class LocalBus : IInMemoryBus
    {
        private static readonly ILog Logger = LogFactory.BuildLogger(typeof(LocalBus));
        private readonly Transport transport;

        public LocalBus(Transport transport)
        {
            this.transport = transport;
        }

        public void Execute(object command)
        {
            Mandate.ParameterNotNull(command, "command");
            //MessageRuleValidation.ValidateCommand(command);

            if (!transport.CurrentMessage.Equals(MessageContext.Null))
                throw new InvalidOperationException("A command may not be executed while another command is being processed.");

            var commandContext = new MessageContext
            {
                Message = command,
                Destination = Address.Local,
                ReplyToAddress = Address.Local,
                MessageType = MessageType.LocalCommand,
                UserName = CurrentUser.GetCurrentUserName()
            };

            transport.HandleIncommingMessage(commandContext);
        }

        public void Raise(object @event)
        {
            if (!ServiceLocator.Current.HasServiceProvider())
                throw new InvalidOperationException("A local event may only be raised within the context of an executing local command or received message.");

            //MessageRuleValidation.ValidateEvent(@event);

            var eventContext = new MessageContext
            {
                Message = @event,
                Destination = Address.Local,
                ReplyToAddress = Address.Local,
                MessageType = MessageType.LocalEvent,
                UserName = CurrentUser.GetCurrentUserName(),
            };

            transport.HandleIncommingMessage(eventContext);
        }
    }
}
