namespace Hermes.Messaging
{
    public interface IMutateIncomingMessages
    {
        object Mutate(object message);
    }
}