using System;
using System.Collections.Generic;

namespace Hermes.Persistence
{
    public interface IUnitOfWork : IDisposable
    {
        void Commit();
        void Rollback();
    }
}