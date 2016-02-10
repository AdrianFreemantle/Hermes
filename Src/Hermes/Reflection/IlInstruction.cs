using System;
using System.Reflection;
using System.Reflection.Emit;
using Hermes.Extensions;

namespace Hermes.Reflection
{
    // ReSharper disable once InconsistentNaming
    public sealed class ILInstruction
    {
        public int Offset { get; set; }
        public OpCode OpCode { get; set; }
        public object Data { get; set; }

        public bool IsMethodCall
        {
            get
            {
                return Data is MethodInfo;
            }
        }

        public bool IsConstructorCall
        {
            get
            {
                return Data is ConstructorInfo;
            }
        }


        public override string ToString()
        {
            return string.Format("{0} : {1} {2}", this.Offset.ToString("X4"), this.OpCode, FormatData());
        }


        private string FormatData()
        {
            if (Data == null) return "";

            MethodInfo methodInfo = this.Data as MethodInfo;
            if (methodInfo != null)
            {
                return methodInfo.ToPrettyString();
            }

            ConstructorInfo constructorInfo = this.Data as ConstructorInfo;
            if (constructorInfo != null)
            {
                return constructorInfo.ToPrettyString();
            }

            throw new NotImplementedException();
        }
    }
}