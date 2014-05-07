using System.Globalization;

namespace Hermes.CronSchedule.Fields
{
    internal class MonthField : CronField
    {
        private const int MonthsInYear = 12;

        private static readonly string[] MonthsOfYearLong = new[] { "JANUARY", "FEBRUARY", "MARCH", "APRIL", "MAY", "JUNE", "JULY", "AUGUST", "SEPTEMBER", "OCTOBER", "NOVEMBER", "DECEMBER" };
        private static readonly string[] MonthsOfYearShort = new[] { "JAN", "FEB", "MAR", "APR", "MAY", "JUN", "JUL", "AUG", "SEP", "OCT", "NOV", "DEC" };

        public MonthField(string expression)
        {
            Parse(expression);
        }

        public override int GetUpperLimitForField()
        {
            return MonthsInYear - 1; //subtract one for zero indexing of months
        }

        public override bool IsNumericOnly()
        {
            return false;
        }

        public override CronValue IndexToValue(int index)
        {
            return new CronValue(index + 1); //expected range is 1 to 12
        }

        public override int ValueToIndex(int currentValue)
        {
            return base.ValueToIndex(currentValue - 1);
        }

        public override string ReplaceTextWithNumbers(string fieldExpression)
        {
            var result = ReplaceTextWithNumbers(fieldExpression, MonthsOfYearLong);
            return ReplaceTextWithNumbers(result, MonthsOfYearShort);
        }

        private static string ReplaceTextWithNumbers(string fieldExpression, string[] replacementData)
        {
            string tempString = fieldExpression.ToUpper();

            for (int i = 0; i < replacementData.Length; i++)
            {
                tempString = tempString.Replace(replacementData[i], (i + 1).ToString(CultureInfo.InvariantCulture));
            }

            return tempString;
        }
    }
}