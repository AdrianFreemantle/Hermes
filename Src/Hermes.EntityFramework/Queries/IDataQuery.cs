using System.Linq;

namespace Hermes.EntityFramework.Queries
{
    public interface IDataQuery
    {
        IQueryable<TEntity> GetQueryable<TEntity>() where TEntity : class;
    }
}