using System;
using System.Globalization;

namespace Hermes.Messaging.Transports
{
    public class HeaderValue
    {
        public string Key { get; private set; }
        public string Value { get; private set; }

        public HeaderValue(string key, string value)
        {
            Mandate.ParameterNotNullOrEmpty(key, "key");
            Mandate.ParameterNotNull(value, "value");

            Key = key;
            Value = value;
        }

        public static HeaderValue FromEnum<TEnum>(string key, TEnum code) where TEnum : struct, IComparable, IFormattable, IConvertible
        {
            return new HeaderValue(key, code.GetHashCode().ToString(CultureInfo.InvariantCulture));
        }

        public static HeaderValue FromString(string key, string value)
        {
            return new HeaderValue(key, value);
        }

        internal void AppendValue(string value)
        {
            Mandate.ParameterNotNull(value, "value");
            Value = String.Format("{0};{1}", Value, value);
        }
    }
}