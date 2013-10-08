using System.Linq;

namespace Hermes.EntityFramework.Queries
{
    public interface IEntityQuery
    {
        IQueryable<TEntity> GetQueryable<TEntity>() where TEntity : class;
    }
}