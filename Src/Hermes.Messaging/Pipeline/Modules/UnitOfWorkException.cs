using System;

namespace Hermes.Messaging.Pipeline.Modules
{
    [Serializable]
    public class UnitOfWorkException : Exception
    {
        public UnitOfWorkException(string message, Exception ex)
            : base(message, ex)
        {
        }
    }
}