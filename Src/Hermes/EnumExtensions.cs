using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Hermes
{
    public static class EnumExtensions
    {
        public static string GetDescription(this Enum value)
        {
            var description = string.Empty;

            try
            {
                var field = GetEnumFieldInfo(value);
                description = GetFieldDescription(field);
            }
            catch (Exception)
            {
                description = "Error! Unable to find a name for this field.";
            }

            return description;
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
            string name = field.Name;
            var attr = Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) as DescriptionAttribute;

            if (!ReferenceEquals(null, attr))
            {
                name = attr.Description;
            }

            return name;
        }

        private static bool FieldHasIgnoreForSelectListAttribute(FieldInfo field)
        {
            var attr = Attribute.GetCustomAttribute(field, typeof(IgnoreForSelectListAttribute)) as IgnoreForSelectListAttribute;

            return (attr != null);
        }
    } 
}
