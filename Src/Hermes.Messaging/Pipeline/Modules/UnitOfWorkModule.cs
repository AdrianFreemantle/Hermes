using System;
using System.Collections.Generic;
using System.Linq;

using Hermes.Logging;
using Hermes.Persistence;
using Hermes.Pipes;

namespace Hermes.Messaging.Pipeline.Modules
{
    public class UnitOfWorkModule : IModule<IncomingMessageContext>
    {
        private static readonly ILog Logger = LogFactory.BuildLogger(typeof(UnitOfWorkModule));
        private readonly IEnumerable<IUnitOfWork> unitsOfWork;

        public UnitOfWorkModule(IEnumerable<IUnitOfWork> unitsOfWork)
        {
            this.unitsOfWork = unitsOfWork;
        }

        public bool Invoke(IncomingMessageContext input, Func<bool> next)
        {
            try
            {
                if (next())
                {
                    CommitUnitsOfWork(input);
                    return true;
                }
                
                RollBackUnitsOfWork(input);
                return false;
            }
            catch
            {
                RollBackUnitsOfWork(input);
                throw;
            }            
        }

        private void CommitUnitsOfWork(IncomingMessageContext input)
        {
            foreach (var unitOfWork in unitsOfWork.Reverse())
            {
                Logger.Debug("Committing {0} unit-of-work for message {1}", unitOfWork.GetType().FullName, input);
                unitOfWork.Commit();
            }
        }

        private void RollBackUnitsOfWork(IncomingMessageContext input)
        {
            foreach (var unitOfWork in unitsOfWork)
            {
                Logger.Debug("Rollback of {0} unit-of-work for message {1}", unitOfWork.GetType().FullName, input);
                unitOfWork.Rollback();
            }
        }
    }
}