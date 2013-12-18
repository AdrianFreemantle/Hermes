using System;

namespace Hermes.Messaging.ProcessManagement
{
    public class ProcessManagerDataNotFoundException : Exception
    {
        public ProcessManagerDataNotFoundException(Guid processManagerId, ProcessManager instance)
            : base(GetMessage(processManagerId, instance))
        {
        }

        private static string GetMessage(Guid processManagerId, ProcessManager instance)
        {
            return String.Format("Process manager {0} : {1} is either complete or has not yet started.", instance.GetType().FullName, processManagerId);
        }
    }
}