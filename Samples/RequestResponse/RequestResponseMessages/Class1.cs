

namespace RequestResponseMessages
{
    public class AddNumbers : ICommand
    {
        public int X { get; set; }
        public int Y { get; set; }
    }

    public interface ICommand
    {
        
    }

    public interface IEvent
    {

    }

    public interface IMessage
    {
        
    }

    public class AdditionResult : IMessage
    {
        public int CalcuationResult { get; set; }
    }

    public interface IResultCalculated : IEvent
    {
        int CalcuationResult { get; set; }
    }

    public class ResultCalculated : IResultCalculated
    {
        public int CalcuationResult { get; set; }
    }

    public enum ErrorCodes
    {
        Success = 0,
        CalculationFault = 1
    }
}
