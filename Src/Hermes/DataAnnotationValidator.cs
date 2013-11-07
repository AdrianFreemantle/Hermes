using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Hermes
{
    public class DataAnnotationValidator 
    {
        public List<ValidationResult> Validate(object o)
        {
            var validationResults = new List<ValidationResult>();
            var vc = new ValidationContext(o, null, null);
            Validator.TryValidateObject(o, vc, validationResults, true);

            return validationResults;
        }
    }
}