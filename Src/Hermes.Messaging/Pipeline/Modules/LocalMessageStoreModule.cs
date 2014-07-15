using System;
using Hermes.Pipes;

namespace Hermes.Messaging.Pipeline.Modules
{
    public class LocalMessageStoreModule : IModule<IncomingMessageContext>
    {
        public bool Process(IncomingMessageContext input, Func<bool> next)
        {
            var session = LocalSession.Begin(input);

            var result = next();
            session.Commit();
            return result;
        }
    }
}