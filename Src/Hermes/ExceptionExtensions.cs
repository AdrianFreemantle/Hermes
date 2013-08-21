using System;
using System.Text;

namespace Hermes
{
    public static class ExceptionExtensions
    {
        public static string GetFullExceptionMessage(this Exception ex)
        {
            var exceptionMessage = new StringBuilder();
            var currentException = ex;

            exceptionMessage.AppendLine();
            exceptionMessage.AppendLine("===================== EXCPTIONS =====================");

            do
            {
                exceptionMessage.AppendLine(String.Format("{0}", currentException.GetType().FullName));
                exceptionMessage.AppendLine(String.Format("{0}", currentException.Message));
                exceptionMessage.AppendLine();

                currentException = currentException.InnerException;
            }
            while (currentException != null);

            exceptionMessage.AppendLine("==================== STACK TRACE ====================");
            exceptionMessage.AppendLine(ex.StackTrace);
            exceptionMessage.AppendLine();

            return exceptionMessage.ToString();
        }
    }
}
