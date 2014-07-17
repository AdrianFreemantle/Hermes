using System.Data.Entity;
using System.Transactions;
using Hermes.Messaging;
using Hermes.Messaging.Configuration;

namespace Hermes.EntityFramework
{
    [InitializationOrder(Order = 1)]
    public abstract class ContextInitializer<T> : INeedToInitializeSomething, IDatabaseInitializer<T>
        where T : DbContext, new() 
    {
        protected readonly string MutexKey;

        protected ContextInitializer()
        {
            MutexKey = typeof(T).FullName;
        }
 
        public virtual void Initialize()
        {
            Database.SetInitializer(this);

            using (var scope = Settings.RootContainer.BeginLifetimeScope())
            {
                var contextFactory = scope.GetInstance<ContextFactory<T>>();
                DbContext context = contextFactory.GetContext();
                context.Database.CompatibleWithModel(false);
            }
        }

        public virtual void InitializeDatabase(T context)
        {
            using (new SingleGlobalInstance(30000, MutexKey))
            {
                if (context.Database.Exists() && !context.Database.CompatibleWithModel(true))
                    throw new IncompatibleDatabaseModelException("The database schema does not match the current model. A migration may be needed to update the database schema.");

                bool created = context.Database.CreateIfNotExists();

                InitializeLookupTables(context);

                if (created)
                {
                    Seed(context);
                }

                context.SaveChanges();
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