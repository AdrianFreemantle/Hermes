using System;

using Hermes.Logging;
using Hermes.Pipes;

namespace Hermes.Messaging.Pipeline.Modules
{
    public class DispatchMessagesModule : IModule<IncomingMessageContext>
    {
        private readonly static ILog Logger = LogFactory.BuildLogger(typeof(DispatchMessagesModule));
        private readonly IDispatchMessagesToHandlers dispatcher;

        public DispatchMessagesModule(IDispatchMessagesToHandlers dispatcher)
        {
            this.dispatcher = dispatcher;
        }

        public bool ExtractMessage(IncomingMessageContext input, Func<bool> next)
        {
            if (!input.IsControlMessage())
            {
                Logger.Debug("Dispatching message {0} to handlers.", input);
               
                dispatcher.DispatchToHandlers(input.Message, input.ServiceLocator);
            }

            return next();
        }
    }
}