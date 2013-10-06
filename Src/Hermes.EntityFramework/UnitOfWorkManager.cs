using System;

using Hermes.Messaging;

namespace Hermes.EntityFramework
{
    public class UnitOfWorkManager : IManageUnitOfWork
    {
        protected readonly IUnitOfWork UnitOfWork;
        private bool disposed;

        public UnitOfWorkManager(EntityFrameworkUnitOfWork unitOfWork)
        {
            UnitOfWork = unitOfWork;
        }

        public void Commit()
        {
            UnitOfWork.Commit();
        }

        public void Rollback()
        {
            UnitOfWork.Rollback();
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
                UnitOfWork.Dispose();
            }

            disposed = true;
        }
    }
}