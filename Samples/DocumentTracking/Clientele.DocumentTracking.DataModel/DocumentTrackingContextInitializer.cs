using System.Data.Entity;

namespace Clientele.DocumentTracking.DataModel
{
    public class DocumentTrackingContextInitializer : IDatabaseInitializer<DocumentTrackingContext>
    {
        public void InitializeDatabase(DocumentTrackingContext context)
        {
            if (context.Database.Exists())
            {
                context.Database.Delete();
            }
        }
    }
}