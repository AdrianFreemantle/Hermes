namespace Contracts
{
    public interface IErrorOccured : IEvent
    {
        string Message { get; }
    }
}
