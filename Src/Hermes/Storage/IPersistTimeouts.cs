namespace Hermes.Storage
{
    public interface IPersistTimeouts
    {
        void Add(TimeoutData timeout);
        bool TryFetchNextTimeout(out TimeoutData timeoutData);
    }
}