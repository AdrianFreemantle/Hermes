using System;
using System.Collections.Generic;
using Hermes.Ioc;
using Hermes.Messaging.Pipeline;
using Microsoft.Practices.ServiceLocation;

namespace Hermes.Messaging.Transports
{
    public class NullMessageContext : IncomingMessageContext
    {
        private NullMessageContext(TransportMessage transportMessage, IServiceLocator serviceLocator) 
            : base(transportMessage, serviceLocator)
        {
        }

        public static NullMessageContext Empty()
        {
            return new NullMessageContext(new TransportMessage(Guid.Empty, Guid.Empty, Address.Undefined, TimeSpan.MinValue, new Dictionary<string, string>(), new byte[0]), new DisposedProvider());
        }
    }
}