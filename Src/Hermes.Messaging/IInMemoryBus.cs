using System.Threading.Tasks;

namespace Hermes.Messaging
{
    public interface IInMemoryBus
    {
        void Execute(object command);
        void Raise(object @event);
    }

    public static class InMemoryBusExtensions
    {
        public static Task ExecuteAsync(this IInMemoryBus localBus, object command)
        {
            return Task.Factory.StartNew(o => localBus.Execute(command), command);
        }
    }
}