using System;
using System.Data.Entity.Validation;
using System.Text;

namespace Hermes.EntityFramework
{
    public class EntityValidationException : Exception
    {
        public EntityValidationException(DbEntityValidationException dbEntityValidationException)
            :base(GetErrorMessage(dbEntityValidationException))
        {
            
        }

        static string GetErrorMessage(DbEntityValidationException dbEntityValidationException)
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("Validation failed for one or more entities.");

            foreach (var dbEntityValidationResult in dbEntityValidationException.EntityValidationErrors)
            {
                AppendErrorMessages(dbEntityValidationResult, stringBuilder);
            }

            return stringBuilder.ToString();
        }

        private static void AppendErrorMessages(DbEntityValidationResult dbEntityValidationResult, StringBuilder stringBuilder)
        {
            var entityName = dbEntityValidationResult.Entry.Entity.GetType().Name;

            foreach (var dbValidationError in dbEntityValidationResult.ValidationErrors)
            {
                string error = String.Format("  Property {0} on entity {1} has validtion error : {2}", dbValidationError.PropertyName, dbValidationError.ErrorMessage,
                                             entityName);
                stringBuilder.AppendLine(error);
            }
        }
    }
}