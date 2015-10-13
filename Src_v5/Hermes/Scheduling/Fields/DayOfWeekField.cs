using System.Globalization;

namespace Hermes.Scheduling.Fields
{
    internal class DayOfWeekField : CronField
    {
        private const int DaysInWeek = 7;

        private static readonly string[] DaysOfWeekLong = new[] { "SUNDAY", "MONDAY", "TUESDAY", "WEDNESDAY", "THURSDAY", "FRIDAY", "SATURDAY" };
        private static readonly string[] DaysOfWeekShort = new[] {"SUN", "MON", "TUE", "WED", "THU", "FRI", "SAT"};

        public DayOfWeekField(string expression)
        {
            Parse(expression);
        }

        public override int GetUpperLimitForField()
        {
            return DaysInWeek - 1; //subtract one for zero indexing of days
        }

        public override bool IsNumericOnly()
        {
            return false;
        }

        public override string ReplaceTextWithNumbers(string fieldExpression)
        {
            var result = ReplaceTextWithNumbers(fieldExpression, DaysOfWeekLong);
            return ReplaceTextWithNumbers(result, DaysOfWeekShort);
        }

        private static string ReplaceTextWithNumbers(string fieldExpression, string[] replacementData)
        {
            string tempString = fieldExpression.ToUpper();

            for (int i = 0; i < replacementData.Length; i++)
            {
                tempString = tempString.Replace(replacementData[i], i.ToString(CultureInfo.InvariantCulture));
            }

            return tempString;
        }
    }
}