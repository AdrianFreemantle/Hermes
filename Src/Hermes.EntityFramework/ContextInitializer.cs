using System.Data.Entity;
using Hermes.Messaging;

namespace Hermes.EntityFramework
{
    [InitializationOrderAttribute(Order = 1)]
    public abstract class ContextInitializer<T> : INeedToInitializeSomething, IDatabaseInitializer<T>
        where T : DbContext
    {
        protected readonly string MutexKey;

        protected ContextInitializer()
        {
            MutexKey = typeof(T).FullName;
        }
 
        public virtual void Initialize()
        {
            Database.SetInitializer(this);
        }

        public virtual void InitializeDatabase(T context)
        {
            using (new SingleGlobalInstance(30000, MutexKey))
            {
                bool created = context.Database.CreateIfNotExists();

                if (!context.Database.CompatibleWithModel(true))
                    throw new IncompatibleDatabaseModelException("The database schema does not match the current model. A migration may be needed to update the database schema.");

                InitializeLookupTables(context);

                if(created)
                    Seed(context);
            }
        }

        protected virtual void InitializeLookupTables(T context)
        {
            
        }

        protected virtual void Seed(T context)
        {
            
        }
    }
}