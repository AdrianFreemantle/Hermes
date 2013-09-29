using System;

namespace Hermes.EntityFramework
{
    public class UnitOfWorkManager : IManageUnitOfWork
    {
        protected readonly IUnitOfWork unitOfWork;
        private bool disposed;

        public UnitOfWorkManager(EntityFrameworkUnitOfWork unitOfWork)
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

        ~UnitOfWorkManager()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
            {
                return;
            }

            if (disposing)
            {
                unitOfWork.Dispose();
            }

            disposed = true;
        }
    }
}