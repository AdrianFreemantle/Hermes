﻿using System;

using Hermes.Messages;

namespace Hermes.Shell
{
    public class MessageHandler 
        : IHandleMessage<SimpleMessage>
            , IHandleMessage<SimpleMessage2>
    {
        public void Handle(SimpleMessage command)
        {
            SimulateTransientError();
        }

        public void Handle(SimpleMessage2 command)
        {
            SimulateTransientError();
        }

        private static void SimulateTransientError()
        {
            //this code is here to simulate a transient error happening on the network
            if (DateTime.Now.Second % 2 == 0)
            {
                throw new Exception("Some random error has happened. This would normally mean our user must retry their action.");
            }
        }
    }
}