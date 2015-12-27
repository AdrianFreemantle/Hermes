using System;
using Hermes.Messaging;

namespace Hermes.Gateway.TestServer
{
    class LocalBusStub : ILocalBus
    {
        public void Execute(IDomainCommand command)
        {
            Console.WriteLine(@"Executing {0}", command.GetType().FullName);
        }

        public void Raise(IDomainEvent @event)
        {
            throw new NotImplementedException();
        }
    }
}