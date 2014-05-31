using Contracts;
using Hermes.Logging;
using Hermes.Messaging;

namespace Remote.Endpoint
{
    public class ErrorMessageHandler : IHandleMessage<IErrorOccured>
    {
        private static readonly ILog Logger = LogFactory.BuildLogger(typeof (ErrorMessageHandler));

        public void Handle(IErrorOccured e)
        {
            Logger.Warn(e.Message);
        }
    }
}