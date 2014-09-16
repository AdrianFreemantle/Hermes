using System.Data.Entity;

namespace LocalBus.Persistence
{
    public class LocalBusTestContext : DbContext
    {
        public IDbSet<Record> Records { get; set; }
        public IDbSet<RecordLog> RecordLogs { get; set; }
        
        public LocalBusTestContext()
        {
        }

        public LocalBusTestContext(string connectionStringName)
            : base(connectionStringName)
        {

        }
    }
}
