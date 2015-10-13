using System.Reflection;

namespace Hermes.Extensions
{
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