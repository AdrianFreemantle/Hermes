using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace Hermes.EntityFramework.SagaPersistence
{
    public class SagaEntityConfiguration : EntityTypeConfiguration<SagaEntity>
    {
        public SagaEntityConfiguration()
        {
            ToTable("SagaData");
            HasKey(entity => entity.Id);
            Property(entity => entity.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);
            Property(entity => entity.State).IsRequired();
            this.HasTimestamp(entity => entity.TimeStamp);
        }
    }
}