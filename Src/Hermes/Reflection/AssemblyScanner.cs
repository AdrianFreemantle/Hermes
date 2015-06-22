using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Hermes.Equality;
using Hermes.Logging;

namespace Hermes.Reflection
{
    public static class AssemblyScanner 
    {
        private static readonly List<Func<string, bool>> ExclusionRules = new List<Func<string, bool>>();
        private static readonly ILog Logger = LogFactory.BuildLogger(typeof(AssemblyScanner));

        private static readonly List<Assembly> assemblies = new List<Assembly>();
        private static readonly List<Type> types = new List<Type>();         

        public static IReadOnlyCollection<Type> Types { get { return types; } }
        public static IReadOnlyCollection<Assembly> Assemblies { get { return assemblies; } }

        static AssemblyScanner()
        {
            AssemblyScannerDefaultIgnoreRules();
            Scan();
        }

        private static void AssemblyScannerDefaultIgnoreRules()
        {
            ExclusionRules.AddRange(new Func<string, bool>[]
            {
                s => s.StartsWith("Topshelf.", StringComparison.CurrentCultureIgnoreCase),
                s => s.StartsWith("mscorlib.", StringComparison.CurrentCultureIgnoreCase),
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

        private static void Scan()
        {
            DirectoryInfo baseDirectory = String.IsNullOrWhiteSpace(AppDomain.CurrentDomain.DynamicDirectory) 
                ? new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory)
                : new DirectoryInfo(AppDomain.CurrentDomain.DynamicDirectory);

            Logger.Debug("Scanning assembly files in location {0}", baseDirectory.FullName);

            var assemblyFiles = baseDirectory.GetFiles("*.dll", SearchOption.AllDirectories)
                                             .Union(baseDirectory.GetFiles("*.exe", SearchOption.AllDirectories))
                                             .Where(info => ExclusionRules.All(func => !func(info.Name)));

            GetTypesFromAssemblies(assemblyFiles);
        }

        private static void GetTypesFromAssemblies(IEnumerable<FileInfo> assemblyFiles)
        {
            foreach (var assemblyFile in assemblyFiles)
            {
                try
                {
                    Logger.Debug("Scanning file {0}", assemblyFile);
                    Assembly assembly = Assembly.LoadFrom(assemblyFile.FullName);
                    types.AddRange(assembly.GetTypes());
                    assemblies.Add(assembly);
                }
                catch (Exception ex)
                {
                    HandleScanningException(ex, assemblyFile);
                }
            }
        }

        private static void HandleScanningException(Exception ex, FileInfo assemblyFile)
        {
            try
            {
                var reflectionTypeLoadException = ex as ReflectionTypeLoadException;

                if (reflectionTypeLoadException != null)
                {
                    var typeLoadExceptions = reflectionTypeLoadException.LoaderExceptions.Select(e => e.GetFullExceptionMessage()).ToArray();
                    var fullExceptionMessage = String.Join("\n", typeLoadExceptions);

                    Logger.Warn(String.Format("Error while scanning assembly {0}\n{1}", assemblyFile.FullName, fullExceptionMessage));
                }
                else
                {
                    Logger.Warn(String.Format("Error while scanning assembly {0}\n{1}", assemblyFile.FullName, ex.GetFullExceptionMessage()));
                }
            }
            catch (Exception exception)
            {
                Logger.Fatal("Error while handling scanning exception: {0}", exception.GetFullExceptionMessage());
            }
        }

        public static ICollection<Type> GetConcreteTypesOf<TAbstract>()
        {
            return GetConcreteTypesOf(typeof (TAbstract));
        }

        public static ICollection<Type> GetConcreteTypesOf(Type abstractType)
        {
            if (!abstractType.IsAbstract)
            {
                return new Type[0];
            }

            return types.Where(t => abstractType.IsAssignableFrom(t) && t != abstractType && !t.IsAbstract).ToArray();
        }

        public static ICollection<Type> GetTypesImplementingGenericInterface(Type openGenericInterface)
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
    }
}