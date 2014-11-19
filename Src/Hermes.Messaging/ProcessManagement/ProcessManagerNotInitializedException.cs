using System;

namespace Hermes.Messaging.ProcessManagement
{
    public class ProcessManagerNotInitializedException : Exception
    {
        public ProcessManagerNotInitializedException(IProcessManager instance)
            : base(GetMessage(instance))
        {
        }

        private static string GetMessage(IProcessManager instance)
        {
            return String.Format("Process manager {0} has not been initialized. Ensure that the Begin, Continue or BeginOrContinue methods have been called.", instance.GetType().FullName);
        }
    }
}