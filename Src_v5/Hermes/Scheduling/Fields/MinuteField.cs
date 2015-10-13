namespace Hermes.Scheduling.Fields
{
    internal class MinuteField : CronField
    {
        private const int MinutesInHour = 60;

        public MinuteField(string expression)
        {
            Parse(expression);
        }

        public override int GetUpperLimitForField()
        {
            return MinutesInHour - 1; //subtract one for zero indexing of minutes
        }

        public override bool IsNumericOnly()
        {
            return true;
        }
    }
}