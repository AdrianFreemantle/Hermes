using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

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

        public string GetDescription()
        {
            if (ValidationResults == null || !ValidationResults.Any())
                return Message;

            StringBuilder builder = new StringBuilder();

            builder.AppendLine(String.Format("A command validation exception has occured: {0}", Message));

            foreach (var validationResult in ValidationResults)
            {
                builder.AppendLine(String.Format("{0}: {1}", validationResult.ErrorMessage, String.Join(", ", validationResult.MemberNames)));
            }

            return builder.ToString();
        }
    }
}
