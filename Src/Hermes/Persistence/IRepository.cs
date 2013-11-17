namespace Hermes.Persistence
{
    public interface IRepository<TEntity> where TEntity : class 
    {
        TEntity Get(object id);
        void Add(TEntity newEntity);
        void Remove(TEntity entity);
    }
}
