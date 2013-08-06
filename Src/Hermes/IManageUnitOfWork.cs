namespace Hermes
{
    public interface IManageUnitOfWork
    {
        void Commit();
        void Rollback();
    }
}