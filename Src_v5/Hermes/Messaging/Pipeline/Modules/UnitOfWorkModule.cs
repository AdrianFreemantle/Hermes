﻿using System;
using System.Collections.Generic;
using System.Linq;
using Hermes.Attributes;
using Hermes.Logging;
using Hermes.Persistence;
using Hermes.Pipes;

namespace Hermes.Messaging.Pipeline.Modules
{
    public class UnitOfWorkModule : IModule<MessageContext>
    {
        private static readonly ILog Logger = LogFactory.BuildLogger(typeof(UnitOfWorkModule));
        private readonly ICollection<IUnitOfWork> unitsOfWork;

        public UnitOfWorkModule(IEnumerable<IUnitOfWork> unitsOfWork)
        {
            this.unitsOfWork = unitsOfWork.ToArray();
        }

        public bool Process(MessageContext input, Func<bool> next)
        {
            try
            {
                var result = next();
                CommitUnitsOfWork(input);
                return result;
            }
            catch (Exception ex)
            {
                RollBackUnitsOfWork(input);
                input.Headers[HeaderKeys.FailureDetails] = ex.GetFullExceptionMessage();
                throw;
            }
        }

        private void CommitUnitsOfWork(MessageContext input)
        {
            foreach (var unitOfWork in OrderedUnitsOfWork())
            {
                Logger.Debug("Committing {0} unit-of-work for message {1}", unitOfWork.GetType().FullName, input);
                unitOfWork.Commit();
            }
        }

        private void RollBackUnitsOfWork(MessageContext input)
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