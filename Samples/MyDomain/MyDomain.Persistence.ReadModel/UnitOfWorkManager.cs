using Hermes;
using MyDomain.Infrastructure;

namespace MyDomain.Persistence.ReadModel
{
    public class UnitOfWorkManager : IManageUnitOfWork
    {
        private readonly IUnitOfWork unitOfWork;

        public UnitOfWorkManager(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }

        public void Commit()
        {
            unitOfWork.Commit();
        }

        public void Rollback()
        {
            unitOfWork.Rollback();
        }
    }
}