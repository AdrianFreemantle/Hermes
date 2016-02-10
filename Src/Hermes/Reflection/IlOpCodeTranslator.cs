using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace Hermes.Reflection
{
    // ReSharper disable once InconsistentNaming
    internal static class ILOpCodeTranslator
    {
        private static readonly Dictionary<short, OpCode> OpCodes = new Dictionary<short, OpCode>();

        static ILOpCodeTranslator()
        {
            Initialize();
        }

        public static OpCode GetOpCode(short value)
        {
            return OpCodes[value];
        }

        private static void Initialize()
        {
            foreach (FieldInfo fieldInfo in typeof(OpCodes).GetFields())
            {
                OpCode opCode = (OpCode)fieldInfo.GetValue(null);

                OpCodes.Add(opCode.Value, opCode);
            }
        }
    }
}