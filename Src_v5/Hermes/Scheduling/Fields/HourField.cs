namespace Hermes.Scheduling.Fields
{
    internal class HourField : CronField
    {
        private const int HoursInDay = 24;

        public HourField(string expression)
        {
            Parse(expression);
        }

        public override int GetUpperLimitForField()
        {
            return HoursInDay - 1; //subtract one for zero indexing of hours
        }

        public override bool IsNumericOnly()
        {
            return true;
        }
    }
}