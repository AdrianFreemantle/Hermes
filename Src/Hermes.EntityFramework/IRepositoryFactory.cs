using System.Data.Entity;

namespace Hermes.EntityFramework
{
    public interface IRepositoryFactory 
    {
        IDbSet<TEntity> GetRepository<TEntity>() where TEntity : class, new();
    }
}