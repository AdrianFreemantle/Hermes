using Hermes.EntityFramework;

namespace EventStore
{
    public class EventStoreContextInitializer : ContextInitializer<EventStoreContext>
    {
        protected override void InitializeLookupTables(EventStoreContext context)
        {
        }

        protected override void Seed(EventStoreContext context)
        {
            //seed any data here
        }
    }
}