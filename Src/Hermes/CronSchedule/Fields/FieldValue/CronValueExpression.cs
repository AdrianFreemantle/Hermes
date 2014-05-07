using System;
using System.Globalization;

namespace Hermes.CronSchedule.Fields.FieldValue
{
    internal class CronValueExpression
    {
        private static readonly SpecialCharacters SpecialCharacters;

        private readonly CronField cronField;

        private const int RangeIndex = 0;
        private const int IncrementIndex = 1;

        public int Increment { get; protected set; }
        public CronRange CronRange { get; protected set; }

        static CronValueExpression()
        {
            SpecialCharacters = SpecialCharacters.ValueExpression();
        }

        public CronValueExpression(string expression, CronField cronField)
        {
            this.cronField = cronField;
            Parse(cronField.ReplaceTextWithNumbers(expression));
        }

        private void Parse(string expression)
        {
            SpecialCharacters.Validate(expression); 
            var splitExpression = expression.Split(SpecialCharacters.IncrementSeperator);

            switch (splitExpression.Length)
            {
                case 2:
                    ParseMultiValue(splitExpression);
                    break;
                case 1:
                    ParseSingleValue(expression);
                    break;
                default:
                    throw new CronException(ErrorMessages.ExpressionFieldFormatError);
            }

            ValidateRangeLimits();
        }

        private void ValidateRangeLimits()
        {
            if (CronRange.UpperLimit > cronField.GetUpperLimitForField())
            {
                var message = String.Format(ErrorMessages.OutOfRangeError, CronRange.UpperLimit, cronField.GetUpperLimitForField());
                throw new CronException(message);
            }
        }

        private void ParseSingleValue(string expression)
        {
            Increment = 1;

            CronRange = IsMatchAll(expression)
                ? new CronRange(cronField.GetUpperLimitForField(), cronField.ValueToIndex(0)) 
                : CronRange.FromExpression(cronField, expression);
        }

        private void ParseMultiValue(string[] splitExpression)
        {
            Increment = GetIncrementFromMultiValue(splitExpression);
            CronRange = GetRangeFromMulitValue(splitExpression);
        }

        private CronRange GetRangeFromMulitValue(string[] splitExpression)
        {
            return IsMatchAll(splitExpression[RangeIndex])
                       ? new CronRange(cronField.GetUpperLimitForField(), cronField.ValueToIndex(0))
                       : CronRange.FromExpression(cronField, splitExpression[RangeIndex]);
        }

        private static int GetIncrementFromMultiValue(string[] splitExpression)
        {
            return Int32.Parse(splitExpression[IncrementIndex]);
        }

        private static bool IsMatchAll(string expression)
        {
            return expression == SpecialCharacters.MatchAll.ToString(CultureInfo.InvariantCulture);
        }
    }
}