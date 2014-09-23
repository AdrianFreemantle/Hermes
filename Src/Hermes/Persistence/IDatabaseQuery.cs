using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

namespace Hermes.Persistence
{
    public interface IDatabaseQuery
    {
        IQueryable<TEntity> GetQueryable<TEntity>() where TEntity : class, new();
        IEnumerable<T> SqlQuery<T>(string sql, params SqlParameter[] parameters);
    }
}