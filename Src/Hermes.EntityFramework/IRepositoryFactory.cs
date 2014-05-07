using System.Data.Entity;
using Hermes.Persistence;

namespace Hermes.EntityFramework
{
    public interface IRepositoryFactory 
    {
        IDbSet<TEntity> GetRepository<TEntity>() where TEntity : class, new();
    }
}