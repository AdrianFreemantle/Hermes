using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using Hermes.EntityFramework;

namespace Hermes.Storage.EntityFramework.ProcessManagager
{
    public class ProcessManagerEntityConfiguration : EntityTypeConfiguration<ProcessManagerEntity>
    {
        public ProcessManagerEntityConfiguration()
        {
            ToTable("ProcessManagerData");
            HasKey(entity => entity.Id);
            Property(entity => entity.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);
            Property(entity => entity.State).IsRequired();
            this.HasTimestamp(entity => entity.TimeStamp);
        }
    }
}