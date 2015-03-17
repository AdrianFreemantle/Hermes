using System.Data.Entity;
using EventStore.Persistence;
using Hermes.EntityFramework;

namespace EventStore
{
    public class EventStoreContext : FrameworkContext
    {
        public IDbSet<EventStream> EventStreams { get; set; }

        public EventStoreContext()
        {
        }

        public EventStoreContext(string connectionStringName)
            : base(connectionStringName)
        {
        }
    }
}