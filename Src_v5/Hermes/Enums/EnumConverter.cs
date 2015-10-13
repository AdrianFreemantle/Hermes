using System;
using System.Collections.Generic;
using System.Linq;

namespace Hermes.Enums
{
    public static class EnumConverter 
    {
        public static Dictionary<TEnum, string> GetAllDescriptions<TEnum>() where TEnum : struct, IComparable, IFormattable, IConvertible
        {
            var enumValues = Enum.GetValues(typeof(TEnum)).Cast<TEnum>();
            var result = new Dictionary<TEnum, string>();

            foreach (TEnum enumValue in enumValues)
            {
                dynamic myDynamicEnum = (TEnum)(dynamic)enumValue;
                var description = ((Enum)myDynamicEnum).GetDescription();

                result.Add(enumValue, description);
            }

            return result;
        }
    }
}