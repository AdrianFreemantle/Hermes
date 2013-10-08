using System;
using System.Diagnostics;

namespace SimpleBank
{
    [Serializable]
    [DebuggerStepThrough]
    public struct Money 
    {
        private readonly decimal amount;

        public static Money Zero { get { return new Money(0); } }

        public Money(decimal amount)
            : this()
        {
            if (amount < 0)
            {
                throw new ArgumentException("A monetary value cannot be smaller than zero");
            }

            this.amount = amount;
        }

        public static Money Amount(decimal amount)
        {
            return new Money(amount);
        }

        public static implicit operator decimal(Money money)
        {
            return money.amount;
        }

        public static Money operator +(Money left, Money right)
        {
            return new Money(left.amount + right.amount);
        }

        public static decimal operator -(Money left, Money right)
        {
            return left.amount - right.amount;
        }

        public override string ToString()
        {
            return String.Format("{0:C}", amount);
        }
    }
}