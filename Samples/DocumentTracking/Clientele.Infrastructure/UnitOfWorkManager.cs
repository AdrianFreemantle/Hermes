using Clientele.Core.Persistance;
using Hermes;

namespace Clientele.Infrastructure
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
