namespace Hermes.Messaging.Deferment
{
    public interface ITimeoutProcessor
    {
        void Start();
        void Stop();
    }
}