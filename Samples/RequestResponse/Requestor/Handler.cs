using System;

using Hermes.Messaging;

using RequestResponseMessages;

namespace Requestor
{
    public class Handler : IHandleMessage<AdditionResult>
    {
        public void Handle(AdditionResult message)
        {
            Console.WriteLine("Result is {0}", message.Result);
        }
    }
}