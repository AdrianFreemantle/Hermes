using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Hermes.Scheduling.Fields.FieldValue;

namespace Hermes.Scheduling.Fields
{
    internal abstract class CronField : ICronField
    {
        private BitArray schedule;
        private static readonly SpecialCharacters SpecialCharacters;

        public static char[] AllowedSpecialCharacters { get; protected set; }
        public abstract int GetUpperLimitForField();
        public abstract bool IsNumericOnly();
        
        static CronField()
        {
            SpecialCharacters = SpecialCharacters.All();
        }

        protected void Parse(string fieldExpression)
        {
            ValidateExpression(fieldExpression);

            IEnumerable<CronValueExpression> fieldValueExpressions = ExtractCronFieldValues(fieldExpression);
            ApplyScheduleFromFieldValues(fieldValueExpressions);
        }

        private void ValidateExpression(string fieldExpression)
        {
            Mandate.ParameterNotNullOrEmpty(fieldExpression, "fieldExpression");

            ValidationOption option = IsNumericOnly() ? ValidationOption.NumericOnly : ValidationOption.AllowLetters;
            SpecialCharacters.Validate(fieldExpression, option);
        }

        private IEnumerable<CronValueExpression> ExtractCronFieldValues(string fieldExpression)
        {           
            var fieldValueExpressions = fieldExpression.Split(SpecialCharacters.ListSeperator);

            return fieldValueExpressions
                .Select(fieldValueExpression => new CronValueExpression(fieldValueExpression, this));
        }

        private void ApplyScheduleFromFieldValues(IEnumerable<CronValueExpression> fieldExpressions)
        {
            schedule = new BitArray(GetUpperLimitForField() + 1);

            foreach (CronValueExpression fieldExpression in fieldExpressions)
            {
                ApplyScheduleRange(fieldExpression.CronRange.LowerLimit, fieldExpression.CronRange.UpperLimit, fieldExpression.Increment);
            }
        }

        private void ApplyScheduleRange(int low, int high, int increment)
        {
            for (int i = low; (i <= high) && (i <= schedule.Length); i += increment)
            {
                schedule.Set(i, true);
            }
        }

        public bool Contains(int value)
        {
            return schedule[ValueToIndex(value)];
        }

        public CronValue GetFirst()
        {
            return GetNext(0);
        }        

        public virtual CronValue GetNext(CronValue current)
        {
            return GetNext(current.Value);
        }

        public virtual CronValue GetNext(int currentValue)
        {
            int currentIndex = ValueToIndex(currentValue);

            for (int i = currentIndex; i < schedule.Length; i++)
            {
                if (schedule[i])
                {
                    return IndexToValue(i);
                }
            }

            return CronValue.Wrapped;
        }

        public virtual int ValueToIndex(int currentValue)
        {
            return currentValue < 0 ? 0 : currentValue;
        }

        public virtual CronValue IndexToValue(int index)
        {
            return new CronValue(index); 
        }

        public virtual string ReplaceTextWithNumbers(string fieldExpression)
        {
            return fieldExpression;
        }
    }
}