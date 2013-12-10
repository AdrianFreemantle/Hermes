using System;
using Hermes.Pipes;

namespace Hermes.Messaging.Pipeline.Modules
{
    public class CallBackHandlerModule : IModule<IncomingMessageContext>
    {
        private readonly IManageCallbacks callBackManager;

        public CallBackHandlerModule(IManageCallbacks callBackManager)
        {
            this.callBackManager = callBackManager;
        }

        public void Invoke(IncomingMessageContext input, Action next)
        {
            callBackManager.HandleCallback(input);
            next();
        }
    }
}