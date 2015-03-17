using Hermes.Ioc;

namespace EventStore
{
    public class EventStoreDependencyRegistrar : IRegisterDependencies
    {
        public void Register(IContainerBuilder containerBuilder)
        {
            containerBuilder.RegisterType<OptimisticEventStore>();
            containerBuilder.RegisterType<OptimisticEventStream>();   
            containerBuilder.RegisterType<PersistStreams>();   
        }
    }
}