using System.Data.Entity;
using Hermes.EntityFramework.MessageStore;

namespace LocalBus.Persistence
{
    public class LocalBusTestContext : DbContext
    {
        public IDbSet<Record> Records { get; set; }
        public IDbSet<RecordLog> RecordLogs { get; set; }
        public IDbSet<MessageStore> MessageStore { get; set; }

        public LocalBusTestContext()
        {
        }

        public LocalBusTestContext(string connectionStringName)
            : base(connectionStringName)
        {

        }
    }
}
