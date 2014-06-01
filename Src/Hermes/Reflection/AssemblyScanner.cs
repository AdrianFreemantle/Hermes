using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using Hermes.Equality;
using Hermes.Logging;

namespace Hermes.Reflection
{
    public static class AssemblyScannerDefaultIgnoreRules
    {
        public static List<Func<string, bool>> Rules { get; private set; }

        static AssemblyScannerDefaultIgnoreRules()
        {
            Rules.AddRange(new Func<string, bool>[]
            {
                s => s.StartsWith("Microsoft.", StringComparison.CurrentCultureIgnoreCase),
                s => s.StartsWith("ServiceStack.", StringComparison.CurrentCultureIgnoreCase),
                s => s.StartsWith("ServiceStack.", StringComparison.CurrentCultureIgnoreCase),
                s => s.StartsWith("Newtonsoft.", StringComparison.CurrentCultureIgnoreCase),
                s => s.StartsWith("System.", StringComparison.CurrentCultureIgnoreCase), 
                s => s.StartsWith("Autofac.", StringComparison.CurrentCultureIgnoreCase),
                s => s.Equals("Antlr3.Runtime.dll", StringComparison.CurrentCultureIgnoreCase),
                s => s.Equals("EntityFramework.dll", StringComparison.CurrentCultureIgnoreCase),
                s => s.Equals("WebGrease.dll", StringComparison.CurrentCultureIgnoreCase),
                s => s.Equals("gsdll32.dll", StringComparison.CurrentCultureIgnoreCase)
            });
        }
    }

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
            AssemblyScannerDefaultIgnoreRules();
            Scan();
        }

        private static void AssemblyScannerDefaultIgnoreRules()
        {
            ExclusionRules.AddRange(new Func<string, bool>[]
            {
                s => s.StartsWith("Microsoft.", StringComparison.CurrentCultureIgnoreCase),
                s => s.StartsWith("ServiceStack.", StringComparison.CurrentCultureIgnoreCase),
                s => s.StartsWith("ServiceStack.", StringComparison.CurrentCultureIgnoreCase),
                s => s.StartsWith("Newtonsoft.", StringComparison.CurrentCultureIgnoreCase),
                s => s.StartsWith("System.", StringComparison.CurrentCultureIgnoreCase), 
                s => s.StartsWith("Autofac.", StringComparison.CurrentCultureIgnoreCase),
                s => s.Equals("Antlr3.Runtime.dll", StringComparison.CurrentCultureIgnoreCase),
                s => s.Equals("EntityFramework.dll", StringComparison.CurrentCultureIgnoreCase),
                s => s.Equals("WebGrease.dll", StringComparison.CurrentCultureIgnoreCase),
                s => s.Equals("gsdll32.dll", StringComparison.CurrentCultureIgnoreCase)
            });
        }

        public static void Exclude(Func<string, bool> exclusionRule)
        {
            Mandate.ParameterNotNull(exclusionRule, "exclusionRule");

            ExclusionRules.Add(exclusionRule);   
        }

        private void Scan()
        {
            DirectoryInfo baseDirectory = String.IsNullOrWhiteSpace(AppDomain.CurrentDomain.DynamicDirectory) 
                ? new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory)
                : new DirectoryInfo(AppDomain.CurrentDomain.DynamicDirectory);

            Logger.Debug("Scanning assembly files in location {0}", baseDirectory.FullName);

            var assemblyFiles = baseDirectory.GetFiles("*.dll", SearchOption.AllDirectories)
                                             .Union(baseDirectory.GetFiles("*.exe", SearchOption.AllDirectories))
                                             .Where(info => ExclusionRules.All(func => !func(info.Name)));

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

        public ICollection<Type> GetConcreteTypesOf<TAbstract>()
        {
            return GetConcreteTypesOf(typeof (TAbstract));
        }

        public ICollection<Type> GetConcreteTypesOf(Type abstractType)
        {
            if (!abstractType.IsAbstract)
            {
                return new Type[0];
            }

            return types.Where(t => abstractType.IsAssignableFrom(t) && t != abstractType && !t.IsAbstract).ToArray();
        }

        public ICollection<Type> GetTypesImplementingGenericInterface(Type openGenericInterface)
        {
            if (!openGenericInterface.IsGenericType || !openGenericInterface.IsInterface)
            {
                return new Type[0];
            }

            return types.Where(
                t => t.GetInterfaces()
                      .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == openGenericInterface))
                        .Distinct(new TypeEqualityComparer()).ToArray();
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
                types.Clear();
            }

            disposed = true;
        }
    }
}