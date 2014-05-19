using System;
using System.Globalization;

namespace Hermes
{
    public static class DateTimeExtensions
    {
        public const string Format = "yyyy-MM-dd HH:mm:ss:ffffff Z";

        /// <summary>
        /// Converts the <see cref="DateTime"/> to a <see cref="string"/> suitable for transport over the wire
        /// </summary>
        /// <returns></returns>
        public static string ToWireFormattedString(this DateTime dateTime)
        {
            return dateTime.ToUniversalTime().ToString(Format, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Converts a wire formatted <see cref="string"/> from <see cref="ToWireFormattedString"/> to a UTC <see cref="DateTime"/>
        /// </summary>
        /// <returns></returns>
        public static DateTime ToUtcDateTime(this string wireFormattedString)
        {
            return DateTime.ParseExact(wireFormattedString, Format, CultureInfo.InvariantCulture).ToUniversalTime();
        }
    }
}
