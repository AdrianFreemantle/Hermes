using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

// ReSharper disable CheckNamespace
namespace System
// ReSharper restore CheckNamespace
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
                return value.Where(Char.IsDigit).ToString();
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        public static string SplitCamelCase(this string input)
        {
            return Regex.Replace(input, "([A-Z])", " $1", RegexOptions.Compiled).Trim();
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

        public static string ToUriSafeString(this string value)
        {
            return Regex.Replace(value, "[^a-zA-Z0-9]", "-");
        }

        public static string ToAlphanumeric(this string value)
        {
            return Regex.Replace(value, "[^a-zA-Z0-9]", "");
        }
    }
}