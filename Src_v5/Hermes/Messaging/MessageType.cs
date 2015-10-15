namespace Hermes.Messaging
{
    public enum MessageType
    {
        Unknown,
        Reply,
        Command,
        LocalCommand,
        Event,
        LocalEvent,
        Control,
        Defer
    }
}
