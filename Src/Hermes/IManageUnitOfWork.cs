using System;

namespace Hermes
{
    public interface IManageUnitOfWork : IDisposable
    {
        void Commit();
        void Rollback();
    }
}