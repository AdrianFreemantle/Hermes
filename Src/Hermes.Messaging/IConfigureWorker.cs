using System;

namespace Hermes.Messaging
{
    public interface IConfigureWorker : IConfigureEndpoint
    {
        IConfigureWorker SecondLevelRetryPolicy(int attempts, TimeSpan delay);
        IConfigureWorker FirstLevelRetryPolicy(int attempts);
    }
}