using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using Hermes.Logging;
using Hermes.Persistence;

namespace Hermes.EntityFramework.Queries
{
    public class DatabaseQuery : IDatabaseQuery
    {
        internal protected static readonly ILog Logger = LogFactory.BuildLogger(typeof(DatabaseQuery));
        private readonly IContextFactory contextFactory;

        public DatabaseQuery(IContextFactory contextFactory)
        {
            this.contextFactory = contextFactory;
        }

        public IQueryable<TEntity> GetQueryable<TEntity>() where TEntity : class
        {
            var context = contextFactory.GetContext();        
            return context.Set<TEntity>().AsNoTracking();
        }

        public IEnumerable<T> SqlQuery<T>(string sql, params SqlParameter[] parameters)
        {
            var context = contextFactory.GetContext();   
            return context.Database.SqlQuery<T>(sql, parameters);
        }
    }
}