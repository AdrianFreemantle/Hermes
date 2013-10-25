using System;

using Hermes.Ioc;

namespace Hermes.Messaging.Configuration
{
    public interface IConfigureWorker : IConfigureEndpoint
    {
        IConfigureWorker SecondLevelRetryPolicy(int attempts, TimeSpan delay);
        IConfigureWorker UseDistributedTransaction();
    }

    public interface IConfigureEndpoint
    {
        IConfigureEndpoint NumberOfWorkers(int numberOfWorkers);
        IConfigureEndpoint FirstLevelRetryPolicy(int attempts, TimeSpan delay);
        IConfigureEndpoint RegisterDependencies(IRegisterDependencies registerationHolder);
        IConfigureEndpoint RegisterMessageRoute<TMessage>(Address endpointAddress);
        IConfigureEndpoint DefineMessageAs(Func<Type, bool> isMessageRule);
        IConfigureEndpoint DefineCommandAs(Func<Type, bool> isCommandRule);
        IConfigureEndpoint DefineEventAs(Func<Type, bool> isEventRule);
        IConfigureEndpoint SubscribeToEvent<TMessage>();
    }
}