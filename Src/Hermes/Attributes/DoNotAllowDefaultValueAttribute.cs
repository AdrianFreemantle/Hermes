using System;
using System.ComponentModel.DataAnnotations;
using Hermes.Reflection;

namespace Hermes.Attributes
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
    public class DoNotAllowDefaultValueAttribute : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            if (value == null)
                return false;

            if (value.GetType().IsValueType)
            {
                object defaultValue = Activator.CreateInstance(value.GetType());
                return !defaultValue.Equals(value);
            }

            return true;
        }
    }
}