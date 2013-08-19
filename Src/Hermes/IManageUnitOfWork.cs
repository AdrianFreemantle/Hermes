using System;

namespace Hermes
{
    public interface IManageUnitOfWork
    {
        //void Register(Action callback);
        void Commit();
        void Rollback();
    }
}