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
            try
            {
                return ProcessMessage(input, next);
            }
            catch (UnitOfWorkRollbackException)
            {
                throw;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        private bool ProcessMessage(IncomingMessageContext input, Func<bool> next)
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

            input.CommitOutgoingMessages();
            FaultSimulator.Trigger();
        }

        private void RollBackUnitsOfWork(IncomingMessageContext input)
        {
            try
            {
                foreach (var unitOfWork in OrderedUnitsOfWork().Reverse())
                {
                    Logger.Debug("Rollback of {0} unit-of-work for message {1}", unitOfWork.GetType().FullName, input);
                    unitOfWork.Rollback();
                    FaultSimulator.Trigger();
                }
            }
            catch (Exception ex)
            {
                throw new UnitOfWorkRollbackException(ex.Message, ex);
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