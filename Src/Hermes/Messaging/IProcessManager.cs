namespace Hermes.Messaging
{
    public interface IProcessManager
    {
        bool IsComplete { get; }
        bool IsNew { get; }
        IMessageBus Bus { get; set; }
        IPersistProcessManagers ProcessManagerPersistence { get; set; }
        void Save();
        IContainProcessManagerData GetCurrentState();
    }
}