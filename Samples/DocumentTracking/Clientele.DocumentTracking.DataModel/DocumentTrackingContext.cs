using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;

using Clientele.DocumentTracking.DataModel.Model;
using Clientele.DocumentTracking.DataModel.ModelConfiguration;

namespace Clientele.DocumentTracking.DataModel
{
    public class DocumentTrackingContext : DbContext
    {
        public DbSet<Document> Documents { get; set; }
        public DbSet<DocumentActivity> DocumentActivity { get; set; }

        public DocumentTrackingContext()
        {
        }

        public DocumentTrackingContext(string databaseName)
            : base(databaseName)
        {
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
            modelBuilder.Configurations.Add(new DocumentConfiguration());
            base.OnModelCreating(modelBuilder);
        }
    }
}