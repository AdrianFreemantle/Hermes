using System;
using System.Data.Entity.Migrations.Model;
using Hermes.Logging;
using Hermes.Messaging;
using Hermes.Messaging.Configuration;

namespace EventStore
{
    public interface ICommand
    {
        
    }

    public class DoSomething : ICommand
    {
        public Guid Id { get; set; }
    }

    public class SomethingDone : IDomainEvent
    {
        public Guid Id { get; set; }
        public long OccuredAtTicks { get; set; }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var endpoint = new Endpoint();
            endpoint.Start();

            var scope = Settings.RootContainer.BeginLifetimeScope();

            var bus = scope.GetInstance<IInMemoryBus>();

            while (true)
            {
                bus.Execute(new DoSomething { Id = SequentialGuid.New() });
                Console.ReadKey();
            }
        }
    }
}
