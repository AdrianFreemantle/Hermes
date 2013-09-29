using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

using Clientele.DocumentTracking.DataModel.Model;

namespace Clientele.DocumentTracking.DataModel.ModelConfiguration
{
    class DocumentConfiguration : EntityTypeConfiguration<Document>
    {
        public DocumentConfiguration()
        {
            HasKey(p => p.Id);

            Property(type => type.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);
        }
    }
}
