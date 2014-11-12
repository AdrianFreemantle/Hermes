using System;
using System.Collections.Generic;
using System.Linq;
using Hermes.Equality;
using Hermes.Ioc;
using Hermes.Messaging.Configuration.MessageHandlerCache;
using Hermes.Queries;
using Hermes.Reflection;

namespace Hermes.Messaging.Configuration
{
    internal static class ComponentScanner
    {
        public static void Scan(IContainerBuilder containerBuilder)
        {
            using (var scanner = new AssemblyScanner())
            {
                IEnumerable<Type> messageTypes = GetMesageTypes(scanner);
                ICollection<Type> messageHandlerTypes = GetMessageHandlerTypes(scanner);
                ICollection<Type> commandValidatorTypes = GetCommandValidatorTypes(scanner);
                ICollection<Type> queryHandlerTypes = GetQueryHandlerTypes(scanner);
                ICollection<Type> intializerTypes = GetInitializerTypes(scanner);

                HandlerCache.InitializeCache(messageTypes, messageHandlerTypes);

                RegisterTypes(containerBuilder, messageHandlerTypes, DependencyLifecycle.InstancePerUnitOfWork);
                RegisterTypes(containerBuilder, queryHandlerTypes, DependencyLifecycle.InstancePerUnitOfWork);
                RegisterTypes(containerBuilder, commandValidatorTypes, DependencyLifecycle.InstancePerUnitOfWork);
                RegisterTypes(containerBuilder, intializerTypes, DependencyLifecycle.SingleInstance);
            }
        }

        private static void RegisterTypes(IContainerBuilder containerBuilder, IEnumerable<Type> types, DependencyLifecycle dependencyLifecycle)
        {
            foreach (var type in types)
            {
                containerBuilder.RegisterType(type, dependencyLifecycle);
            }
        }

        private static IEnumerable<Type> GetMesageTypes(AssemblyScanner scanner)
        {
            return scanner.Types
                          .Where(Settings.IsCommandType)
                          .Union(scanner.Types.Where(Settings.IsEventType))
                          .Union(scanner.Types.Where(Settings.IsMessageType))
                          .Distinct(new TypeEqualityComparer());
        }

        private static ICollection<Type> GetMessageHandlerTypes(AssemblyScanner scanner)
        {
            return
                scanner.Types.Where(
                    t => !t.IsAbstract &&  
                        t.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IHandleMessage<>)))
                       .Distinct(new TypeEqualityComparer())
                       .ToArray();
        }

        private static ICollection<Type> GetCommandValidatorTypes(AssemblyScanner scanner)
        {
            return
                scanner.Types.Where(
                    t => !t.IsAbstract &&
                        t.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IValidateCommand<>)))
                       .Distinct(new TypeEqualityComparer())
                       .ToArray();
        }

        private static ICollection<Type> GetQueryHandlerTypes(AssemblyScanner scanner)
        {
            return scanner.Types.Where(
                    t => !t.IsAbstract && 
                        t.GetInterfaces().Any(i => i.IsGenericType && (i.GetGenericTypeDefinition() == typeof(IEntityQuery<,>))))
                       .Distinct(new TypeEqualityComparer())
                       .ToArray();
        }

        private static ICollection<Type> GetInitializerTypes(AssemblyScanner scanner)
        {
            return scanner.Types
                .Where(t => typeof (INeedToInitializeSomething).IsAssignableFrom(t) && !t.IsAbstract)
                .ToArray();
        }
    }
}