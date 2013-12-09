using System;
using System.Collections.Generic;
using Microsoft.Practices.ServiceLocation;

namespace Hermes.Pipes
{
    public class ModuleStack<T>
    {
        private readonly List<Type> processChain = new List<Type>();

        public ModuleStack<T> Add<TProcessor>() where TProcessor : IModule<T>
        {
            processChain.Add(typeof(TProcessor));
            return this;
        }

        public ModuleChain<T> ToProcessChain(IServiceLocator serviceLocator)
        {
            var chain = new Queue<Type>();

            foreach (var type in processChain)
            {
                chain.Enqueue(type);
            }

            return new ModuleChain<T>(chain, serviceLocator);
        }
    }
}