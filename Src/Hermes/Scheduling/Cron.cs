using Hermes.Scheduling.Fields;

namespace Hermes.Scheduling
{
    //  CRON EXPRESSION
    //  ────────────────────────────────────────────────────────────────────────────────
    //  ┌────┬────┬────┬────┬────┬─ Cron Field-Expressions
    //  |    |    |    |    |    |
    //  ┴    ┴    ┴    ┴    ┴    ┴
    //  *    *    *    *    *    *  ├── Cron Expression
    //  ┬    ┬    ┬    ┬    ┬    ┬
    //  │    │    │    │    │    │
    //  │    │    │    │    │    └── day of week (0 - 6) (0 = Sunday) (Mon - Sun) (Monday - Sunday)
    //  │    │    │    │    └─────── month (1 - 12) (1 = January) (Jan - Dec) (January - December)
    //  │    │    │    └──────────── day of month (1 - 31)
    //  │    │    └───────────────── hour (0 - 23)
    //  │    └────────────────────── min (0 - 59)
    //  └─────────────────────────── sec (0 - 59) 
    //
    //  ALLOWED SPECIAL CHARACTERS
    //  ────────────────────────────────────────────────────────────────────────────────
    //  Asterisk ( * )
    //  The asterisk indicates that the cron expression will match for all values of the field.
    //  For example, using an asterisk in the 4th field (month) would indicate every month.
    //
    //  Slash ( / )
    //  Slashes are used to describe increments of ranges. 
    //  For example 3-59/15 in the 1st field (minutes) would indicate the 3rd minute of the hour and every 15 minutes thereafter. 
    //  The form "*/..." is equivalent to the form "first-last/...", that is, an increment over the largest possible range of the field.
    //  The form "*/10" in the 1st field (minutes) would indicate every 10 minutes, i.e. minutes 0, 10, 20, 30, 40 and 50.
    //
    //  Comma ( , )
    //  Commas are used to separate items of a list. 
    //  For example, using "1,3,5" in the 5th field (day of week) would mean Mondays, Wednesdays and Fridays.
    //
    //  Hyphen ( - )
    //  Hyphens are used to define ranges. 
    //  For example, 1-5 in the 5th field (day of week) would indicate every week day, Monday to Friday.
    //
    //  The above information is derived from http://en.wikipedia.org/wiki/Cron
   
    public class Cron
    {
        private const int SecondIndex = 0;
        private const int MinuteIndex = 1;
        private const int HourIndex = 2;
        private const int DayOfMonthIndex = 3;
        private const int MonthIndex = 4;
        private const int DayOfWeekIndex = 5;

        private const int RequiredFieldCount = 6;

        private Cron()
        {
        }

        public static CronSchedule Parse(string cronExpression)
        {
            Mandate.ParameterNotNullOrEmpty(cronExpression, "cronExpression");

            var fieldExpressions = GetFieldExpressions(cronExpression);

            return BuildCronSchedule(fieldExpressions);
        }

        private static string[] GetFieldExpressions(string cronExpression)
        {
            var splitExpression = cronExpression.Split(' ');

            if (splitExpression.Length != RequiredFieldCount)
            {
                throw new CronException(string.Format(ErrorMessages.ExpressionFieldCountError, RequiredFieldCount));
            }

            return splitExpression;
        }

        private static CronSchedule BuildCronSchedule(string[] fieldExpressions)
        {
            var secondField = new SecondField(fieldExpressions[SecondIndex]);
            var minuteField = new MinuteField(fieldExpressions[MinuteIndex]);
            var hourField = new HourField(fieldExpressions[HourIndex]);
            var dayOfMonthField = new DayOfMonthField(fieldExpressions[DayOfMonthIndex]);
            var monthField = new MonthField(fieldExpressions[MonthIndex]);
            var dayOfWeek = new DayOfWeekField(fieldExpressions[DayOfWeekIndex]);

            return new CronSchedule(secondField, minuteField, hourField, dayOfMonthField, monthField, dayOfWeek);
        }
    }
}