using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace Hermes.EntityFramework.Queries
{
    public class DatabaseQuery : IDatabaseQuery, IDataQuery
    {
        private readonly DbContext context;
 
        public DatabaseQuery(IContextFactory contextFactory)
        {
            context = contextFactory.GetContext();
        }

        public IEnumerable<TDto> SqlQuery<TDto>(string sqlQuery, params object[] parameters) 
        {
            return context.Database.SqlQuery<TDto>(sqlQuery, parameters);
        }

        public IQueryable<TEntity> GetQueryable<TEntity>() where TEntity : class
        {
            return context.Set<TEntity>();
        }
    }
}