using System.Threading.Tasks;
using System.ServiceModel;

using Hermes.Logging;
using Hermes.Messaging.Configuration;

namespace Hermes.Messaging.Wcf
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public abstract class CommandService
    {
        private static readonly ILog Logger = LogFactory.BuildLogger(typeof(CommandService));
        private readonly IMessageBus messageBus;

        protected CommandService()
        {
            messageBus = Settings.RootContainer.GetInstance<IMessageBus>();
        }

        public virtual async Task<int> Put<TCommand>(TCommand command)
        {
            Logger.Verbose("Received message {0}", command.ToString());
            Task<int> myOrderCallback = messageBus.Send(command).Register(c => c.ErrorCode);
            return await myOrderCallback;
        }
    }
}
