using System;

using Hermes.Logging;
using Hermes.Pipes;

namespace Hermes.Messaging.Pipeline.Modules
{
    public class CallBackHandlerModule : IModule<IncomingMessageContext>
    {
        private readonly IManageCallbacks callBackManager;
        private readonly static ILog Logger = LogFactory.BuildLogger(typeof(CallBackHandlerModule));

        public CallBackHandlerModule(IManageCallbacks callBackManager)
        {
            this.callBackManager = callBackManager;
        }

        public bool Process(IncomingMessageContext input, Func<bool> next)
        {
            Logger.Verbose("Attempting to dispatch message {0} to registered callbacks.", input);
            callBackManager.HandleCallback(input);
            return next();
        }
    }
}