using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

using Hermes.Logging;

namespace Hermes.Reflection
{
    public class AssemblyScanner : IDisposable
    {
        private static readonly List<Func<string, bool>> ExclusionRules = new List<Func<string, bool>>();
        private static readonly ILog Logger = LogFactory.BuildLogger(typeof(AssemblyScanner));

        private readonly List<Assembly> assemblies = new List<Assembly>();
        private readonly List<Type> types = new List<Type>();         
        private bool disposed;      

        public IReadOnlyCollection<Type> Types { get { return types; } }
        public IReadOnlyCollection<Assembly> Assemblies { get { return assemblies; } }

        public AssemblyScanner()
        {
            Scan();
        }

        public static void Exclude(Func<string, bool> exclusionRule)
        {
            Mandate.ParameterNotNull(exclusionRule, "exclusionRule");

            ExclusionRules.Add(exclusionRule);   
        }

        private void Scan()
        {
            var baseDirectory = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
            Logger.Debug("Scanning assembly files in location {0}", baseDirectory.FullName);

            var assemblyFiles = baseDirectory.GetFiles("*.dll", SearchOption.AllDirectories)
                                             .Union(baseDirectory.GetFiles("*.exe", SearchOption.AllDirectories))
                                             .Where(info => ExclusionRules.All(func => func(info.FullName)));

            foreach (var assemblyFile in assemblyFiles)
            {
                try
                {
                    Logger.Debug("Scanning file {0}", assemblyFile);
                    Assembly assembly = Assembly.LoadFrom(assemblyFile.FullName);
                    assembly.GetTypes();
                    assemblies.Add(assembly);
                    types.AddRange(assembly.GetTypes());
                }
                catch (Exception ex)
                {
                    Logger.Error(ex.Message);
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