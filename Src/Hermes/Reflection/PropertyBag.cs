using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Hermes.Reflection
{
    public class PropertyBag : IReadOnlyCollection<PropertyInfo>
    {
        private const BindingFlags Flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
        private readonly ICollection<PropertyInfo> properties;

        public IEnumerator<PropertyInfo> GetEnumerator()
        {
            return properties.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public int Count { get { return properties.Count; } }

        public PropertyBag(Type type)
        {
            Mandate.ParameterNotNull(type, "type");

            properties = type.GetProperties(Flags);
        }

        public PropertyInfo GetPropertyWithAttribute<TAttribute>() where TAttribute : Attribute
        {
            return properties.Single(info => info.GetCustomAttribute<TAttribute>() != null);
        }

        public PropertyInfo GetPropertyWithName(string propertyName)
        {
            Mandate.ParameterNotNullOrEmpty(propertyName, "propertyName");

            return properties.Single(info => info.Name == propertyName);
        }
    }

    public static class PropertyInfoExtensions
    {
        public static T GetPropertyValue<T>(this PropertyInfo propertyInfo, object obj)
        {
            return (T)GetPropertyValue(propertyInfo, obj);
        }

        public static object GetPropertyValue(this PropertyInfo propertyInfo, object obj)
        {
            Mandate.ParameterNotNull(obj, "obj");
            return propertyInfo.GetValue(obj, null);
        }

        public static void SetPropertyValue(this PropertyInfo propertyInfo, object obj, object val)
        {
            Mandate.ParameterNotNull(obj, "obj");
            propertyInfo.SetValue(obj, val);
        }       
    }

}