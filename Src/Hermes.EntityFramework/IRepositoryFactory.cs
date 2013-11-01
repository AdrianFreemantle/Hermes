namespace Hermes.EntityFramework
{
    public interface IRepositoryFactory 
    {
        EntityFrameworkRepository<TEntity> GetRepository<TEntity>() where TEntity : class;
    }
}