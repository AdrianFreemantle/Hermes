using System.Data.Entity;
using IntegrationTest.Client.Persistence;

namespace IntegrationTest.Client.Wireup
{
    public class DataBaseCreationPolicy : DropCreateDatabaseAlways<LocalBusTestContext>
    {
        
    }
}