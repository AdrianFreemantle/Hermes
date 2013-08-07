using Hermes.Ioc;

namespace Hermes.Core
{
    public class LocalBus : IInMemoryBus
    {
        private readonly IContainer container;
        private readonly IDispatchMessagesToHandlers messageDispatcher;

        public LocalBus(IDispatchMessagesToHandlers messageDispatcher, IContainer container)
        {
            this.container = container;
            this.messageDispatcher = messageDispatcher;
        }

        public void Raise(object messageBody)
        {
            messageDispatcher.DispatchToHandlers(container, messageBody);
        }
    }
}