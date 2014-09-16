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
        /// <summary>
        /// This is used for validation of commands being executed on the local bus
        /// </summary>
        public bool Process(IncomingMessageContext input, Func<bool> next)
        {
            if (!input.IsControlMessage() && Settings.IsCommandType(input.Message.GetType()))
                ValidateCommand(input.Message, input.ServiceLocator);

            return next();
        }

        public bool Process(OutgoingMessageContext input, Func<bool> next)
        {
            if (input.OutgoingMessageType == OutgoingMessageContext.MessageType.Command)
                ValidateCommand(input.OutgoingMessage, input.ServiceLocator);

            return next();
        }

        public void ValidateCommand(object command, IServiceLocator serviceLocator)
        {
            Mandate.ParameterNotNull(command, "command");
            Mandate.ParameterNotNull(serviceLocator, "serviceLocator");

            var results = DataAnnotationValidator.Validate(command);

            if (results.Any())
                throw new CommandValidationException(results);

            if (Settings.EnableCommandValidationClasses)
            {
                ValidateUsingValidatorClass(command, serviceLocator);
            }
        }

        private static void ValidateUsingValidatorClass(object command, IServiceLocator serviceLocator)
        {
            object validator;
            var validatorType = typeof (IValidateCommand<>).MakeGenericType(command.GetType());

            if (serviceLocator.TryGetInstance(validatorType, out validator))
            {
                ((dynamic)validator).Validate((dynamic)command);
            }
        }
    }
}