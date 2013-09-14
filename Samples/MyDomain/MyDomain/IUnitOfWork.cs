namespace MyDomain
{
    public interface IUnitOfWork
    {
        void Commit();
        void Rollback();
        IRepository<TEntity> GetRepository<TEntity>() where TEntity : class;
    }
}