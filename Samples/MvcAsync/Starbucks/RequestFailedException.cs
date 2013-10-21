using System;
using Hermes;
using Starbucks.Messages;

namespace Starbucks
{
    [Serializable]
    public class RequestFailedException : Exception
    {
        public RequestFailedException(ErrorCodes errorCode)
            : base(GetErrorMessage(errorCode))
        {
            
        }

        static string GetErrorMessage(ErrorCodes errorCode)
        {
            return String.Format("Request failed with error: {0}", errorCode.GetDescription());
        }
    }
}