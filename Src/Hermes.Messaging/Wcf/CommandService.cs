﻿using System.Threading.Tasks;
using System.ServiceModel;

using Hermes.Messaging.Configuration;

namespace Hermes.Messaging.Wcf
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public abstract class CommandService<TCommand> : ICommandService<TCommand>
    {
        private readonly IMessageBus messageBus;

        protected CommandService()
        {
            messageBus = Settings.RootContainer.GetInstance<IMessageBus>();
        }

        public virtual async Task<int> Execute(TCommand command)
        {
            Task<int> myOrderCallback = messageBus.Send(command).Register(c => c.ErrorCode);
            return await myOrderCallback;
        }
    }
}
