using System;
using System.Collections.Generic;
using Hermes.Logging;
using Microsoft.Practices.ServiceLocation;

namespace Hermes.Pipes
{
    public class ModuleChain<T>
    {
        private readonly ILog logger;
        private readonly Queue<Type> chain;
        private readonly IServiceLocator serviceLocator;

        public ModuleChain(Queue<Type> chain, IServiceLocator serviceLocator)
        {
            logger = LogFactory.BuildLogger(GetType());
            this.chain = chain;
            this.serviceLocator = serviceLocator;
        }

        public virtual bool Invoke(T input)
        {
            return InvokeNext(input);
        }

        protected virtual bool InvokeNext(T input)
        {
            if (chain.Count == 0)
            {
                return true;
            }

            var processor = (IModule<T>)serviceLocator.GetInstance(chain.Dequeue());

            logger.Debug("Invoking module {0}", processor.GetType().FullName);
            var result = processor.ExtractMessage(input, () => InvokeNext(input));
            logger.Debug("Returning module {0} with result [{1}]", processor.GetType().FullName, result);
            return result;
        }
    }
}