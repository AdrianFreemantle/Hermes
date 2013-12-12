using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Hermes
{
    public static class StringExtensions
    {
        public static bool IsAllDigits(this string value)
        {
            return value.All(Char.IsDigit);
        }

        public static bool IsAllLetters(this string value)
        {
            return value.All(Char.IsLetter);
        }

        public static string GetAllDigits(this string value)
        {
            try
            {
                return Regex.Match(value, @"\d+").Value;
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        public static string Trim(this string value, string valueToTrim)
        {
            return value.EndsWith(valueToTrim, StringComparison.InvariantCultureIgnoreCase) 
                ? value.Substring(0, value.Length - valueToTrim.Length) 
                : value;
        }

        public static bool Contains(this string value, IEnumerable<char> characters)
        {
            return characters.Any(value.ToList().Remove);
        }
    }
}