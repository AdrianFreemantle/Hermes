using System;
using System.Linq;
using Hermes.Ioc;
using Hermes.Messaging.Configuration;
using Hermes.Messaging.Transports;
using Hermes.Pipes;
using Microsoft.Practices.ServiceLocation;

namespace Hermes.Messaging.Pipeline.Modules
{
    public class CommandValidationModule : 
        IModule<IncomingMessageContext>, 
        IModule<OutgoingMessageContext>
    {
        public bool Process(IncomingMessageContext input, Func<bool> next)
        {
            if (input.IsLocalMessage || input.IsControlMessage() || !Settings.IsCommandType(input.Message.GetType()))
                return true;

            ValidateCommand(input, input.ServiceLocator);

            return false;
        }

        public bool Process(OutgoingMessageContext input, Func<bool> next)
        {
            if (input.OutgoingMessageType == OutgoingMessageContext.MessageType.Command)
                return true;

            ValidateCommand(input.OutgoingMessage, input.ServiceLocator);
            return true;
        }

        public void ValidateCommand(object command, IServiceLocator serviceLocator)
        {
            Mandate.ParameterNotNull(command, "command");
            Mandate.ParameterNotNull(serviceLocator, "serviceLocator");

            var results = DataAnnotationValidator.Validate(command);

            if (results.Any())
                throw new CommandValidationException(results);

            object validator;
            var validatorType = typeof(IValidateCommand<>).MakeGenericType(command.GetType());

            if (serviceLocator.TryGetInstance(validatorType, out validator))
            {
                ((dynamic)validator).Validate(command);
            }
        }
    }
}