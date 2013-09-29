using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Data.Entity.ModelConfiguration.Configuration;
using System.Linq.Expressions;

namespace Hermes.EntityFramework
{
    public static class EntityTypeConfigurationExtensions
    {
        public static BinaryPropertyConfiguration HasTimestamp<TEntity>(
            this EntityTypeConfiguration<TEntity> entityRoot,
            Expression<Func<TEntity, byte[]>> property) where TEntity : class
        {
            return entityRoot.Property(property)
                             .HasColumnType("timestamp")
                             .IsRequired()
                             .IsConcurrencyToken()
                             .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Computed);
        }
    }
}