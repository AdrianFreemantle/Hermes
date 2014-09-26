using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Hermes.Messaging
{
    public class CommandValidationException : Exception
    {
        private readonly ValidationResult[] validationResults;

        public IEnumerable<ValidationResult> ValidationResults 
        {
            get { return validationResults; }
        }

        public CommandValidationException(IEnumerable<ValidationResult> validationResults)
            : this("A validation exception has occured", validationResults.ToList())
        {
        }

        public CommandValidationException(List<ValidationResult> validationResults)
            : this("A validation exception has occured", validationResults)
        {
        }

        public CommandValidationException(string message, ValidationResult validationResults)
            : this(message, new[] { validationResults })
        {
        }

        public CommandValidationException(ValidationResult validationResults)
            : this("A validation exception has occured", validationResults)
        {
        }

        public CommandValidationException(string message, IEnumerable<ValidationResult> validationResults)
            : base(message)
        {
            this.validationResults = validationResults.ToArray();
        }
    }
}
