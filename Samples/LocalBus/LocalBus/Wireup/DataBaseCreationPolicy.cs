using System.Data.Entity;
using LocalBus.Persistence;

namespace LocalBus.Wireup
{
    public class DataBaseCreationPolicy : DropCreateDatabaseAlways<LocalBusTestContext>
    {
        
    }
}