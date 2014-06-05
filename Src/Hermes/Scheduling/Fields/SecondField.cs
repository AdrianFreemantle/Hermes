namespace Hermes.Scheduling.Fields
{
    internal class SecondField : CronField
    {
        private const int SecondsInMinute = 60;

        public SecondField(string expression)
        {
            Parse(expression);
        }

        public override int GetUpperLimitForField()
        {
            return SecondsInMinute - 1; //subtract one for zero indexing of seconds
        }

        public override bool IsNumericOnly()
        {
            return true;
        }
    }
}