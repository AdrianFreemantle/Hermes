using System;
using System.ComponentModel.DataAnnotations.Schema;
using Hermes.Persistence;

namespace Hermes.Enums
{
    /// <summary>
    /// a Generic lookup table wrapper for Enum lookup table values
    /// </summary>
    /// <typeparam name="TEnum">The enum type to be wrapped</typeparam>
    public abstract class EnumWrapper<TEnum> : ILookupTable
        where TEnum : struct, IComparable, IFormattable, IConvertible
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public virtual int Id { get; protected set; }
        public virtual TEnum Enum { get { return ((TEnum)((dynamic)Id)); } }

        public virtual string Description
        {
            get { return ToString(); }
            set
            {
                //We get the description from the enum, and therefore we dont want to actually have a setter.
                //However without this setter, this column would not be created in the database by entity framework
                //So to fool entity framework we have an empty setter.
            }
        }

        protected EnumWrapper()
        {
        }

        protected EnumWrapper(TEnum value)
        {
            SetIdValue(value);
        }

        private void SetIdValue(TEnum value)
        {
            //the dynamic is required to cast generic enum types to an int
            Id = (int)(dynamic)value;
        }

        /// <summary>
        /// Retrieves the human readable description from the enum
        /// </summary>
        public override string ToString()
        {
            dynamic myDynamicEnum = (TEnum)(dynamic)Id;
            return ((Enum)myDynamicEnum).GetDescription();
        }

        /// <summary>
        /// Overidden to assist with entity framework equality comparisons
        /// </summary>
        public override bool Equals(object obj)
        {
            var other = obj as EnumWrapper<TEnum>;

            if (ReferenceEquals(other, null))
                return false;

            return Id == other.Id;
        }

        /// <summary>
        /// Overidden to assist with entity framework equality comparisons
        /// </summary>
        public override int GetHashCode()
        {
            return Id;
        }

        public static implicit operator int(EnumWrapper<TEnum> wrapper)
        {
            return wrapper.Id;
        }

        public static implicit operator string(EnumWrapper<TEnum> wrapper)
        {
            return wrapper.Description;
        }

        public static implicit operator TEnum(EnumWrapper<TEnum> wrapper)
        {
            if (ReferenceEquals(wrapper, null))
                return default(TEnum);

            return wrapper.Enum;
        }
    }
}