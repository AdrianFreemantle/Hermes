using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using MyDomain.Persistence.ReadModel.Models;

namespace MyDomain.Persistence.ReadModel
{
    public class MyDomainContext : DbContext 
    {
        public DbSet<ClaimEvent> ClaimEvents { get; set; }

        public MyDomainContext()
        {
        }

        public MyDomainContext(string databaseName)
            : base(databaseName)
        {
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
            base.OnModelCreating(modelBuilder);
        }
    }
}

