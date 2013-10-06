using System;
using System.Globalization;

namespace Hermes.Messaging
{
    public class HeaderValue
    {
        public string Key { get; private set; }
        public string Value { get; private set; }

        public HeaderValue(string key, string value)
        {
            Mandate.ParameterNotNullOrEmpty(key, "key");
            Mandate.ParameterNotNullOrEmpty(value, "value");

            Key = key;
            Value = value;
        }

        public static HeaderValue FromEnum<TEnum>(string key, TEnum errorCode) where TEnum : struct, IComparable, IFormattable, IConvertible
        {
            return new HeaderValue(key, errorCode.GetHashCode().ToString(CultureInfo.InvariantCulture));
        }
    }
}