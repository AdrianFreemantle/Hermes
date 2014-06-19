﻿using System;

using Hermes.Ioc;

namespace Hermes.Messaging
{
    public interface IConfigureEndpoint
    {
        IConfigureEndpoint DontUseDistributedTransaction();
        IConfigureEndpoint NumberOfWorkers(int numberOfWorkers);
        IConfigureEndpoint RegisterDependencies(IRegisterDependencies registerationHolder);
        IConfigureEndpoint RegisterDependencies<T>() where T : IRegisterDependencies, new();
        IConfigureEndpoint RegisterMessageRoute<TMessage>(Address endpointAddress);
        IConfigureEndpoint DefineMessageAs(Func<Type, bool> isMessageRule);
        IConfigureEndpoint DefineCommandAs(Func<Type, bool> isCommandRule);
        IConfigureEndpoint DefineEventAs(Func<Type, bool> isEventRule);
        IConfigureEndpoint SendOnlyEndpoint();
        IConfigureEndpoint UserNameResolver(Func<string> resolveUserName);
    }
}