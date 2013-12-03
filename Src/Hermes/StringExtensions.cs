using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace Hermes
{
    public static class StringExtensions
    {
        public static bool HasText(this string value)
        {
            return !String.IsNullOrEmpty(value) && !String.IsNullOrWhiteSpace(value);
        }

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

        public static string Trim(this string current, string valueToTrim)
        {
            return current.Substring(0, current.Length - valueToTrim.Length);
        }
    }
}