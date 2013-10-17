using Hermes;

namespace Starbucks.Messages
{
    public class BuyCoffee : ICommand
    {
    }

    public enum ErrorCodes
    {
        Success = 0,
        Error = 1
    }

    public interface ICommand
    {

    }
}
