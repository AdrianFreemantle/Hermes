using Hermes.Persistence;

namespace Hermes.EntityFramework
{
    public interface IRepositoryFactory 
    {
        IRepository<TEntity> GetRepository<TEntity>() where TEntity : class, new();
    }
}