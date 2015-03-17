using System;
using Hermes.Messaging;

namespace EventStore
{
    public class Handler : IHandleMessage<DoSomething>, IHandleMessage<SomethingDone>
    {
        private readonly IInMemoryBus bus;
        private readonly IStoreEvents store;

        public Handler(IInMemoryBus bus, IStoreEvents store)
        {
            this.bus = bus;
            this.store = store;
        }

        public void Handle(DoSomething m)
        {
            bus.Raise(new SomethingDone { Id = m.Id, OccuredAtTicks = DateTime.Now.Ticks });
        }

        public void Handle(SomethingDone m)
        {
            using (var stream = store.OpenStream("claims", m.Id))
            {
                stream.Add(new EventMessage { Body = m });
                stream.CommitChanges(SequentialGuid.New());
            }
        }
    }
}