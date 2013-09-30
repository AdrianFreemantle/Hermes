using Hermes.Messaging;

namespace Hermes.Saga
{
    public interface ISaga<out T> where T : class, IContainSagaData
    {
        IPersistSagas SagaPersistence { get; set; }
        IMessageBus Bus { get; set; }
    }
}