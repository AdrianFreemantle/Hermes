using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using Hermes.Configuration;

namespace Hermes.EntityFramework.SagaPersistence
{
    public static class EntityFrameworkSagaPersistenceConfiguration
    {
        public static IConfigureEndpoint UseEntityFrameworkSagaPersister(this IConfigureEndpoint config)
        {

            return config;
        }
    }


    internal class SagaEntityConfiguration : EntityTypeConfiguration<SagaEntity>
    {
        public SagaEntityConfiguration()
        {
            ToTable("SagaData");
            HasKey(entity => entity.Id);
            Property(entity => entity.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);
            Property(entity => entity.State).IsRequired();
            HasKey(entity => entity.TimeStamp);
            HasKey(entity => entity.TimeStamp);
        }
    }
}