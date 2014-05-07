namespace Hermes.CronSchedule.Fields
{
    internal class DayOfMonthField : CronField
    {
        private const int DaysInMonth = 31;

        public DayOfMonthField(string expression)
        {
            Parse(expression);
        }

        public override int GetUpperLimitForField()
        {
            return DaysInMonth - 1; //subtract one for zero indexing of days
        }

        public override bool IsNumericOnly()
        {
            return true;
        }

        public override CronValue IndexToValue(int index)
        {
            return new CronValue(index + 1); //expected range is 1 to 31
        }

        public override int ValueToIndex(int currentValue)
        {
            return base.ValueToIndex(currentValue - 1);
        }
    }
}