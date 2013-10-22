using System;

namespace Hermes.Persistence
{
    public interface IUnitOfWork : IDisposable
    {
        void Commit();
        void Rollback();
    }
}