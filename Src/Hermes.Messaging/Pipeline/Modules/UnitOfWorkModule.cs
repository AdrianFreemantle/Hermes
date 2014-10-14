using System;
using System.Collections.Generic;
using System.Linq;
using Hermes.Attributes;
using Hermes.Failover;
using Hermes.Logging;
using Hermes.Persistence;
using Hermes.Pipes;

namespace Hermes.Messaging.Pipeline.Modules
{
    public class UnitOfWorkModule : IModule<IncomingMessageContext>
    {
        private static readonly ILog Logger = LogFactory.BuildLogger(typeof(UnitOfWorkModule));
        private readonly ICollection<IUnitOfWork> unitsOfWork;        

        public UnitOfWorkModule(IEnumerable<IUnitOfWork> unitsOfWork)
        {
            this.unitsOfWork = unitsOfWork.ToArray();
        }

        public bool Process(IncomingMessageContext input, Func<bool> next)
        {
            try
            {
                var result = next();
                CommitUnitsOfWork(input);                
                return result;
            }
            catch (Exception ex)
            {
                Logger.Error("Error on message {0} {1}", input.TransportMessage.MessageId, ex.GetFullExceptionMessage());
                RollBackUnitsOfWork(input);
                input.TransportMessage.Headers[HeaderKeys.FailureDetails] = ex.GetFullExceptionMessage();
                throw;
            }
        }

        private void CommitUnitsOfWork(IncomingMessageContext input)
        {
            foreach (var unitOfWork in OrderedUnitsOfWork())
            {
                Logger.Debug("Committing {0} unit-of-work for message {1}", unitOfWork.GetType().FullName, input);
                unitOfWork.Commit();
                FaultSimulator.Trigger();
            }
        }

        private void RollBackUnitsOfWork(IncomingMessageContext input)
        {
            foreach (var unitOfWork in OrderedUnitsOfWork().Reverse())
            {
                Logger.Warn("Rollback of {0} unit-of-work for message {1}", unitOfWork.GetType().FullName, input);
                unitOfWork.Rollback();
            }
        }

        private IEnumerable<IUnitOfWork> OrderedUnitsOfWork()
        {
            return unitsOfWork
                .Where(something => something.HasAttribute<InitializationOrderAttribute>())
                .OrderBy(i => i.GetCustomAttributes<InitializationOrderAttribute>().First().Order)
                .Union(unitsOfWork.Where(i => !i.HasAttribute<InitializationOrderAttribute>()));
        }
    }
}