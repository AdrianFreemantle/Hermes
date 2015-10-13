using Hermes.Ioc;
using Hermes.Messaging;

namespace Hermes.Domain
{
    internal static class AggregateEventBus
    {
        public static void Raise(IAggregateEvent e)
        {
            var eventBus = ServiceLocator.Current.GetInstance<IInMemoryEventBus>();
            eventBus.Raise(e);
        }
    }
}