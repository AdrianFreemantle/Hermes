using Hermes.Persistence;

namespace Hermes.EntityFramework
{
    public interface IRepositoryFactory 
    {
        IQueryableRepository<TEntity> GetRepository<TEntity>() where TEntity : class, new();
    }
}