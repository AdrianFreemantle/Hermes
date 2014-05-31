using Contracts;

namespace LocalBus.Contracts
{
    public class ErrorOccured : IErrorOccured
    {
        public string Message { get; private set; }

        public ErrorOccured(string message)
        {
            Message = message;
        }
    }
}