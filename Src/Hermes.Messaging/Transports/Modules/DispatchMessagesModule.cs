﻿using System;
using Hermes.Pipes;

namespace Hermes.Messaging.Transports.Modules
{
    public class DispatchMessagesModule : IModule<IncomingMessageContext>
    {
        private readonly IDispatchMessagesToHandlers dispatcher;

        public DispatchMessagesModule(IDispatchMessagesToHandlers dispatcher)
        {
            this.dispatcher = dispatcher;
        }

        public void Invoke(IncomingMessageContext input, Action next)
        {
            if (!input.IsControlMessage())
            {
                foreach (var message in input.Messages)
                {
                    dispatcher.DispatchToHandlers(message, input.ServiceLocator);
                }
            }

            next();
        }
    }
}