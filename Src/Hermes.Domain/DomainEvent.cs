using Hermes.Ioc;
using Hermes.Messaging;

namespace Hermes.Domain
{
    public static class DomainEvent
    {
        public static void Raise(IDomainEvent e)
        {
            var eventBus = ServiceLocator.Current.GetInstance<IInMemoryEventBus>();
            eventBus.Raise(e);
        }
    }
}