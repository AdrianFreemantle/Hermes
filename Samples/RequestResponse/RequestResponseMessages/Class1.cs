
using Hermes;

namespace RequestResponseMessages
{
    public class AddNumbers : Command
    {
        public int X { get; set; }
        public int Y { get; set; }
    }

    public class AdditionResult : IMessage
    {
        public int Result { get; set; }
    }

    public enum ErrorCodes
    {
        Success = 0,
        CalculationFault = 1
    }
}
