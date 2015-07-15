using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;

namespace Hermes.Messaging.Transports
{
    [DataContract]
    public class HeaderValue
    {
        [DataMember(Name = "Key")]
        public string Key { get; set; }

        [DataMember(Name = "Value")]
        public string Value { get; set; }

        public HeaderValue()
        {}

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

        public static HeaderValue FromKeyValue(KeyValuePair<string, string> keyValuePair)
        {
            return new HeaderValue(keyValuePair.Key, keyValuePair.Value);
        }
    }
}