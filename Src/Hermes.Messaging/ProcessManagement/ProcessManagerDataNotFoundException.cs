using System;

namespace Hermes.Messaging.ProcessManagement
{
    public class ProcessManagerDataNotFoundException : Exception
    {
        public ProcessManagerDataNotFoundException(Guid processManagerId, IProcessManager instance)
            : base(GetMessage(processManagerId, instance))
        {
        }

        public ProcessManagerDataNotFoundException(IProcessManager instance)
            : base(GetMessage(instance))
        {
        }

        private static string GetMessage(Guid processManagerId, IProcessManager instance)
        {
            return String.Format("Process manager {0} : {1} is either complete or has not yet started.", instance.GetType().FullName, processManagerId);
        }

        private static string GetMessage(IProcessManager instance)
        {
            return String.Format("Process manager {0} is either complete or has not yet started.", instance.GetType().FullName);
        }
    }
}