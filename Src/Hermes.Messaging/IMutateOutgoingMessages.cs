namespace Hermes.Messaging
{
    public interface IMutateOutgoingMessages
    {
        object Mutate(object message);
    }
}