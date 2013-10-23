using System.Data.Entity;
using System.Linq;

namespace Hermes.EntityFramework.Queries
{
    public class DatabaseQuery : ISqlQuery, IEntityQuery
    {
        private readonly DbContext context;
 
        public DatabaseQuery(IContextFactory contextFactory)
        {
            context = contextFactory.GetContext();

            context.Configuration.AutoDetectChangesEnabled = false;
            context.Configuration.LazyLoadingEnabled = false;
            context.Configuration.ProxyCreationEnabled = false;
        }

        public SqlQueryResult<TDto> SqlQuery<TDto>(string sqlQuery, params object[] parameters)
        {
            var result = context.Database.SqlQuery<TDto>(sqlQuery, parameters);
            return new SqlQueryResult<TDto>(result);
        }

        public IQueryable<TEntity> GetQueryable<TEntity>() where TEntity : class
        {
            return context.Set<TEntity>();
        }        
    }
}