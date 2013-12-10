using System;
using System.Collections.Generic;
using System.Linq;
using Hermes.Persistence;
using Hermes.Pipes;

namespace Hermes.Messaging.Pipeline.Modules
{
    public class UnitOfWorkModule : IModule<IncomingMessageContext>
    {
        private readonly IEnumerable<IUnitOfWork> unitsOfWork;

        public UnitOfWorkModule(IEnumerable<IUnitOfWork> unitsOfWork)
        {
            this.unitsOfWork = unitsOfWork;
        }

        public void Invoke(IncomingMessageContext input, Action next)
        {
            try
            {
                next();
                CommitUnitsOfWork();                
            }
            catch
            {
                RollBackUnitsOfWork();
                throw;
            }
        }

        private void CommitUnitsOfWork()
        {
            foreach (var unitOfWork in unitsOfWork.Reverse())
            {
                unitOfWork.Commit();
            }
        }

        private void RollBackUnitsOfWork()
        {
            foreach (var unitOfWork in unitsOfWork)
            {
                unitOfWork.Rollback();
            }
        }
    }
}