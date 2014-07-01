using System;
using Hermes.Pipes;

namespace Hermes.Messaging.Pipeline.Modules
{
    public class LocalMessageStoreModule : IModule<IncomingMessageContext>
    {
        public bool Process(IncomingMessageContext input, Func<bool> next)
        {
            var session = LocalSession.Begin(input);

            try
            {
                return next();
            }
            catch (Exception ex)
            {
                session.AddErrorDetails(ex);
                return false;
            }
            finally
            {
                session.Commit();
            }
        }
    }
}