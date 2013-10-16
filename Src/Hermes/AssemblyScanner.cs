using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

using Hermes.Logging;

namespace Hermes
{
    public class AssemblyScanner : IDisposable
    {
        readonly List<Assembly> assemblies = new List<Assembly>();
        readonly List<Type> types = new List<Type>(); 
        static readonly ILog logger = LogFactory.BuildLogger(typeof (AssemblyScanner));
        bool disposed;

        public IReadOnlyCollection<Type> Types { get { return types; } }
        public IReadOnlyCollection<Assembly> Assemblies { get { return assemblies; } }

        public AssemblyScanner()
        {
            Scan();
        }

        private void Scan()
        {
            var baseDirectory = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
            logger.Verbose("Scanning assembly files in location {0}", baseDirectory.FullName);

            var assemblyFiles = baseDirectory.GetFiles("*.dll", SearchOption.AllDirectories)
                .Union(baseDirectory.GetFiles("*.exe", SearchOption.AllDirectories));

            foreach (var assemblyFile in assemblyFiles)
            {
                try
                {
                    logger.Verbose("Scanning file {0}", assemblyFile);
                    Assembly assembly = Assembly.LoadFrom(assemblyFile.FullName);
                    assembly.GetTypes();
                    assemblies.Add(assembly);
                    types.AddRange(assembly.GetTypes());
                }
                catch (Exception ex)
                {
                    logger.Error(ex.Message);
                }
            }
        }

        public IEnumerable<Type> GetConcreteTypesOf<TAbstract>()
        {
            var abstractType = typeof(TAbstract);

            if (!abstractType.IsAbstract)
            {
                return new Type[0];
            }

            return types.Where(t => abstractType.IsAssignableFrom(t) && t != abstractType && !t.IsAbstract);
        }

        ~AssemblyScanner()
        {
            Dispose(false);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
            {
                return;
            }

            if (disposing)
            {
                assemblies.Clear();
            }

            disposed = true;
        }
    }
}