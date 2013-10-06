using System;

namespace Hermes.Messaging
{
    public interface IManageUnitOfWork : IDisposable
    {
        void Commit();
        void Rollback();
    }
}