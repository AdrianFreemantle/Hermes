using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;

using Hermes.EntityFramework;
using Hermes.Storage.EntityFramework.KeyValueStore;

using SimpleBank.DataModel.ReadModel;

namespace SimpleBank.DataModel
{   
    public class SimpleBankContext : FrameworkContext
    {
        public IDbSet<PortfolioRecord> Portfolios { get; set; }
        public IDbSet<AccountRecord> Accounts { get; set; }
        public IDbSet<AccountTypeLookup> AccountType { get; set; }
        public IDbSet<TransactionRecord> TransactionRecords { get; set; }

        public SimpleBankContext()
        {
        }

        public SimpleBankContext(string databaseName)
            : base(databaseName)
        {
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();

            modelBuilder.Configurations.Add(new KeyValueEntityConfiguration());
            modelBuilder.Configurations.Add(new PortfolioRecordConfiguration());
            modelBuilder.Configurations.Add(new AccountRecordConfiguration());

            base.OnModelCreating(modelBuilder);
        }
    }
}