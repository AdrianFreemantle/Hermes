using System;
using System.ComponentModel;
using System.Reflection;
using Hermes.Attributes;

namespace Hermes.Enums
{
    public static class EnumExtensions
    {
        public static string GetDescription(this Enum value)
        {
            try
            {
                var field = GetEnumFieldInfo(value);
                return GetFieldDescription(field);
            }
            catch (Exception)
            {
                return String.Format("Error: Value {0} is undefined.", ((int)(dynamic)value));
            }
        }        

        public static bool HasIgnoreForSelectListAttribute(this Enum value)
        {
            try
            {
                var field = GetEnumFieldInfo(value);
                return FieldHasIgnoreForSelectListAttribute(field);
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static T TryGetCustomAttribute<T>(this Enum value) where T : Attribute
        {
            var field = GetEnumFieldInfo(value);
            return Attribute.GetCustomAttribute(field, typeof(T)) as T;
        }

        private static FieldInfo GetEnumFieldInfo(Enum value)
        {
            Type type = value.GetType();
            var name = Enum.GetName(type, value);
            var field = type.GetField(name);
            return field;
        }

        private static string GetFieldDescription(FieldInfo field)
        {
            var attr = Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) as DescriptionAttribute;

            if (ReferenceEquals(null, attr))
            {
                return field.Name.SplitCamelCase();
            }

            return attr.Description;
        }  

        private static bool FieldHasIgnoreForSelectListAttribute(FieldInfo field)
        {
            var attr = Attribute.GetCustomAttribute(field, typeof(IgnoreForSelectListAttribute)) as IgnoreForSelectListAttribute;

            return (attr != null);
        }
    }
}
