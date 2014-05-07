using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Hermes.CronSchedule
{
    internal enum ValidationOption
    {
        NumericOnly,
        AllowLetters,
    }

    internal class SpecialCharacters
    {
        public const char MatchAll = '*';
        public const char IncrementSeperator = '/';
        public const char ListSeperator = ',';
        public const char RangeSeperator = '-';

        private static readonly string RangeSeperatorString;
        private static readonly string MatchAllString;
        private static readonly string IncrementSeperatorString;

        private readonly ICollection<char> allowedSpecialCharacters;

        static SpecialCharacters()
        {
            RangeSeperatorString = RangeSeperator.ToString(CultureInfo.InvariantCulture);
            MatchAllString = MatchAll.ToString(CultureInfo.InvariantCulture);
            IncrementSeperatorString = IncrementSeperator.ToString(CultureInfo.InvariantCulture);
        }

        private SpecialCharacters(IEnumerable<char> allowedSpecialCharacters)
        {
            this.allowedSpecialCharacters = allowedSpecialCharacters.ToList();
        }

        public static SpecialCharacters All()
        {
            return new SpecialCharacters(GetAll());
        }

        public static SpecialCharacters ValueExpression()
        {
            return new SpecialCharacters(new[] { MatchAll, IncrementSeperator, RangeSeperator });
        }

        private static IEnumerable<char> GetAll()
        {
            return new[] { MatchAll, IncrementSeperator, ListSeperator, RangeSeperator };
        }

        public bool IsValid(string expression)
        {
            return IsValid(expression, ValidationOption.NumericOnly);
        }

        public bool IsValid(string expression, ValidationOption validationOption)
        {
            Mandate.ParameterNotNullOrEmpty(expression, "expression");

            ValidateSpecialCharacterFormat(expression);

            IEnumerable<char> specialCharacters = expression.ToCharArray();

            specialCharacters = validationOption == ValidationOption.AllowLetters
                ? specialCharacters.Where(c => !Char.IsLetterOrDigit(c))
                : specialCharacters.Where(c => !Char.IsDigit(c));

            return specialCharacters.All(specialCharacter => allowedSpecialCharacters.Contains(specialCharacter));
        }

        private static void ValidateSpecialCharacterFormat(string expression)
        {
            if (expression.Length > 1)
            {
                if (expression.StartsWith(RangeSeperatorString) || expression.EndsWith(RangeSeperatorString))
                {
                    throw new CronException(ErrorMessages.StartsOrEndsWithRangeError);
                }

                if (expression.StartsWith(IncrementSeperatorString) || expression.EndsWith(IncrementSeperatorString))
                {
                    throw new CronException(ErrorMessages.StartsOrEndsWithIncrementError);
                }

                if (expression.EndsWith(MatchAllString))
                {
                    throw new CronException(ErrorMessages.EndsWithMatchAllError);
                }
            }
            else 
            {
                if (expression == RangeSeperatorString)
                {
                    throw new CronException(ErrorMessages.OnlyRangeSeperatorError);
                }

                if (expression == IncrementSeperatorString)
                {
                    throw new CronException(ErrorMessages.OnlyIncrementSeperatorError);
                }
            }
        }

        public void Validate(string expression)
        {
            if (!IsValid(expression, ValidationOption.NumericOnly))
            {
                throw new CronException(ErrorMessages.InvalidCharacterError);
            }
        }

        public void Validate(string expression, ValidationOption validationOption)
        {
            if (!IsValid(expression, validationOption))
            {
                throw new CronException(ErrorMessages.InvalidCharacterError);
            }
        }
    }
}