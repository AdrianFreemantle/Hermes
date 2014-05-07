using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Linq;

namespace Hermes.EntityFramework.Queries
{
    public class DatabaseQuery
    {
        private readonly IContextFactory contextFactory;
        protected DbContext Context;

        public DatabaseQuery(IContextFactory contextFactory)
        {
            this.contextFactory = contextFactory;
        }

        public IQueryable<TEntity> GetQueryable<TEntity>() where TEntity : class, new()
        {
            var context = GetDbContext();
            return context.Set<TEntity>();
        }

        public DbRawSqlQuery<T> SqlQuery<T>(string sql, params SqlParameter[] parameters)
        {
            var context = GetDbContext();
            return context.Database.SqlQuery<T>(sql, parameters);
        }

        protected DbContext GetDbContext()
        {
            if (Context == null)
            {
                Context = contextFactory.GetContext();
                Context.Configuration.AutoDetectChangesEnabled = false;
                Context.Configuration.LazyLoadingEnabled = false;
                Context.Configuration.ProxyCreationEnabled = false;
            }

            return Context;
        }
    }
}