using System;
using System.Globalization;

namespace EventStore
{
    public sealed class LongCheckpoint : ICheckpoint, IComparable<LongCheckpoint>
    {
        private readonly long value;

        public LongCheckpoint(long value)
        {
            this.value = value;
        }

        public string Value { get { return value.ToString(CultureInfo.InvariantCulture); } }

        public long LongValue { get { return value; } }

        public int CompareTo(ICheckpoint other)
        {
            if (other == null)
            {
                return 1;
            }
            var longCheckpoint = other as LongCheckpoint;
            if (longCheckpoint == null)
            {
                throw new InvalidOperationException("Can only compare with {0} but compared with {1}"
                    .FormatWith(typeof(LongCheckpoint).Name, other.GetType()));
            }
            return value.CompareTo(longCheckpoint.LongValue);
        }

        public static LongCheckpoint Parse(string checkpointValue)
        {
            return string.IsNullOrWhiteSpace(checkpointValue) ? new LongCheckpoint(-1) : new LongCheckpoint(long.Parse(checkpointValue));
        }

        public int CompareTo(LongCheckpoint other)
        {
            return LongValue.CompareTo(other.LongValue);
        }
    }
}