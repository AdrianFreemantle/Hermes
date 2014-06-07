using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using Hermes.Failover;
using Hermes.Logging;
using Hermes.Messaging.Configuration;
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
            using (var scope = StartTransactionScope())
            {
                var result = ProcessMessage(input, next);
                scope.Complete();
                return result;
            }
        }

        private bool ProcessMessage(IncomingMessageContext input, Func<bool> next)
        {
            try
            {
                next();
            }
            catch(Exception ex)
            {
                Logger.Error("Error on message {0} {1}", input.TransportMessage.MessageId, ex.GetFullExceptionMessage());
                RollBackUnitsOfWork(input);
                input.TransportMessage.Headers[HeaderKeys.FailureDetails] = ex.GetFullExceptionMessage();
                return false;
            }

            CommitUnitsOfWork(input);
            return true;
        }

        private void CommitUnitsOfWork(IncomingMessageContext input)
        {
            foreach (var unitOfWork in OrderedUnitsOfWork())
            {
                Logger.Debug("Committing {0} unit-of-work for message {1}", unitOfWork.GetType().FullName, input);
                unitOfWork.Commit();
                FaultSimulator.Trigger();
            }

            input.CommitOutgoingMessages();
        }
        private void RollBackUnitsOfWork(IncomingMessageContext input)
        {
            foreach (var unitOfWork in OrderedUnitsOfWork().Reverse())
            {
                Logger.Debug("Rollback of {0} unit-of-work for message {1}", unitOfWork.GetType().FullName, input);
                unitOfWork.Rollback();
                FaultSimulator.Trigger();
            }
        }

        private IEnumerable<IUnitOfWork> OrderedUnitsOfWork()
        {
            return unitsOfWork
                .Where(something => something.HasAttribute<InitializationOrderAttribute>())
                .OrderBy(i => i.GetCustomAttributes<InitializationOrderAttribute>().First().Order)
                .Union(unitsOfWork.Where(i => !i.HasAttribute<InitializationOrderAttribute>()));
        }

        protected virtual TransactionScope StartTransactionScope()
        {
            if (Settings.UseDistributedTransaction)
            {
                Logger.Debug("Beginning a transaction scope with option[Required]");
                return TransactionScopeUtils.Begin(TransactionScopeOption.Required);
            }

            Logger.Debug("Beginning a transaction scope with option[Suppress]");
            return TransactionScopeUtils.Begin(TransactionScopeOption.Suppress);
        }
    }
}