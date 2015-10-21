using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Hermes.Ioc;
using Hermes.Pipes;

namespace Hermes.Messaging.Pipeline.Modules
{
    public class CommandValidationModule : IModule<MessageContext>
    {
        /// <summary>
        /// This is used for validation of commands being executed on the local bus
        /// </summary>
        public bool Process(MessageContext input, Func<bool> next)
        {
            if (input.IsCommandMessage())
                ValidateCommand(input.Message);

            return next();
        }

        public void ValidateCommand(object command)
        {
            Mandate.ParameterNotNull(command, "command");

            var validationResults = new List<ValidationResult>();
            var vc = new ValidationContext(command, null, null);
            Validator.TryValidateObject(command, vc, validationResults, true);

            if (validationResults.Any())
                throw new CommandValidationException(validationResults);

            ValidateUsingValidatorClass(command);
        }

        private static void ValidateUsingValidatorClass(object command)
        {
            object validator;
            var validatorType = typeof(IValidateCommand<>).MakeGenericType(command.GetType());

            if (ServiceLocator.Current.TryGetInstance(validatorType, out validator))
            {
                ((dynamic)validator).Validate((dynamic)command);
            }
        }
    }
}