using System;
using System.Runtime.Serialization;

namespace Hermes.CronSchedule
{
    [Serializable]
    public class CronException : Exception
    {
        public CronException() :
            base("Cron error.")
        {
        }

        public CronException(string message) :
            base(message)
        {
        }

        public CronException(string message, Exception innerException) :
            base(message, innerException)
        {
        }

        protected CronException(SerializationInfo info, StreamingContext context) :
            base(info, context)
        {
        }
    }
}