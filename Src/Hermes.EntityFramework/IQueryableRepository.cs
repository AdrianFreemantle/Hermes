using System.Linq;

using Hermes.Persistence;

namespace Hermes.EntityFramework
{
    public interface IQueryableRepository<TEntity> : IRepository<TEntity>, IQueryable<TEntity> where TEntity : class
    {
    }
}
