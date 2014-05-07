using System;

namespace Hermes.CronSchedule.Fields
{
    internal class CronValue : IEquatable<CronValue>
    {       
        private const int WrapValue = -1;
        internal static CronValue Wrapped { get { return new CronValue(WrapValue); } }

        public int Value { get; private set; }
        public bool HasWrapped { get { return Value == WrapValue; } }

        internal CronValue(int value)
        {
            Value = value;
        }

        public static CronValue From(int value)
        {
            return new CronValue(value);
        }

        public bool Equals(CronValue other)
        {
            if (other != null)
            {
                return other.Value == Value;
            }

            return false;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }
            if (ReferenceEquals(this, obj))
            {
                return true;
            }
            if (obj.GetType() != GetType())
            {
                return false;
            }
            return Equals((CronValue)obj);
        }

        public override int GetHashCode()
        {
            return Value;
        }

        public static bool operator ==(CronValue left, CronValue right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(CronValue left, CronValue right)
        {
            return !Equals(left, right);
        }
    }
}