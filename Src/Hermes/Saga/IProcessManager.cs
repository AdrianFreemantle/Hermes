using Hermes.Messaging;

namespace Hermes.Saga
{
    public interface IProcessManager<out T> where T : class, IContainProcessManagerData
    {
        IPersistProcessManagers ProcessManagerPersistence { get; set; }
        IMessageBus Bus { get; set; }
    }
}