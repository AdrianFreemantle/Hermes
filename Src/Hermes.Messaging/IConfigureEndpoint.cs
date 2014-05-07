using System;

using Hermes.Ioc;

namespace Hermes.Messaging
{
    public interface IConfigureEndpoint
    {
        IConfigureEndpoint DontUseDistributedTransaction();
        IConfigureEndpoint NumberOfWorkers(int numberOfWorkers);
        IConfigureEndpoint RegisterDependencies(IRegisterDependencies registerationHolder);
        IConfigureEndpoint RegisterMessageRoute<TMessage>(Address endpointAddress);
        IConfigureEndpoint DefineMessageAs(Func<Type, bool> isMessageRule);
        IConfigureEndpoint DefineCommandAs(Func<Type, bool> isCommandRule);
        IConfigureEndpoint DefineEventAs(Func<Type, bool> isEventRule);
        IConfigureEndpoint SendOnlyEndpoint();
        IConfigureEndpoint UserIdResolver(Func<string> resolveUserName);
    }
}