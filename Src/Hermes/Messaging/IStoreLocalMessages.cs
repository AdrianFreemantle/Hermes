namespace Hermes.Messaging
{
    public interface IStoreLocalMessages
    {
        void SaveSession(LocalSession session);
    }
}