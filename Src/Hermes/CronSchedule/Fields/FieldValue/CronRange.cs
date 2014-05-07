using System;

namespace Hermes.CronSchedule.Fields.FieldValue
{
    internal class CronRange
    {
        private const int LowerIndex = 0;
        private const int UpperIndex = 1;

        public int UpperLimit { get; private set; }
        public int LowerLimit { get; private set; }

        public CronRange(int upperLimit, int lowerLimit)
        {
            Mandate.That(upperLimit >= lowerLimit, "The upper limit value must be larger or equal to the lower limit value");

            UpperLimit = upperLimit;
            LowerLimit = lowerLimit;
        }

        public static CronRange FromExpression(CronField cronField, string rangeExpression)
        {
            Mandate.ParameterNotNullOrEmpty(rangeExpression, "rangeExpression");
            int parsed;

            if(Int32.TryParse(rangeExpression, out parsed))
            {
                return new CronRange(cronField.ValueToIndex(parsed), cronField.ValueToIndex(parsed));
            }

            return ParseRangeExpression(cronField, rangeExpression);
        }

        private static CronRange ParseRangeExpression(CronField cronField, string rangeExpression)
        {
            try
            {
                var range = rangeExpression.Split('-');
                return new CronRange(cronField.ValueToIndex(Int32.Parse(range[UpperIndex])), cronField.ValueToIndex(Int32.Parse(range[LowerIndex])));
            }
            catch (FormatException ex)
            {
                throw new CronException("The range expression is incorrectly formatted.", ex);
            }
        }
    }
}