using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace EventDriven
{
    public static class ActivatorHelper
    {
        const BindingFlags Flags = BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public;

        public static T CreateInstance<T>(params object[] parameters) where T : class
        {
            if (parameters.Length == 0)
                return Activator.CreateInstance(typeof(T)) as T;

            return Activator.CreateInstance(typeof(T), parameters) as T;
        }

        public static T CreateInstanceUsingNonPublicConstructor<T>(params object[] parameters) where T : class
        {
            Type[] types = parameters.ToList().ConvertAll(input => input.GetType()).ToArray();

            var constructor = typeof(T).GetConstructor(Flags, null, types, null);

            return constructor.Invoke(parameters) as T;
        }

        public static List<TBase> CreateInstancesImplimentingBase<TBase>(string assemblyName) where TBase : class
        {
            var assembly = Assembly.Load(assemblyName);
            var concreteSubTypes = GetConcreteSubTypes<TBase>(assembly);
            return CreateInstancesImplimentingBase<TBase>(concreteSubTypes);
        }

        public static List<TBase> CreateInstancesImplimentingBase<TBase>() where TBase : class
        {
            var assembly = Assembly.GetAssembly(typeof(TBase));
            var concreteSubTypes = GetConcreteSubTypes<TBase>(assembly);
            return CreateInstancesImplimentingBase<TBase>(concreteSubTypes);
        }

        public static List<TBase> CreateInstancesImplimentingBase<TBase>(IEnumerable<Type> concreteTypes) where TBase : class
        {
            return concreteTypes.Select(type => Activator.CreateInstance(type) as TBase).ToList();
        }

        public static IEnumerable<Type> GetConcreteSubTypes<TBase>(Assembly sourceAssembly) where TBase : class
        {
            var assignableFromTypes = sourceAssembly.GetTypes().Where(type => type.IsAssignableFrom(typeof(TBase)));
            var implimentingInterfaceTypes = sourceAssembly.GetTypes().Where(type => type.GetInterfaces().Contains(typeof(TBase)));

            return assignableFromTypes.Union(implimentingInterfaceTypes).Where(type => !type.IsAbstract && type.IsClass).ToList();
        }
    }
}