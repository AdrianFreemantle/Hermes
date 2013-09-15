using System.Collections.Generic;

namespace Hermes.Messaging
{
    public interface IInMemoryBus
    {
        //void Execute(params object[] commands);
        void Raise(params object[] events);
    }
}