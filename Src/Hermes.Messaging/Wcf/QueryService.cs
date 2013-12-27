using System.ServiceModel;
using System.Threading.Tasks;

using Hermes.Logging;
using Hermes.Messaging.Configuration;
using Hermes.Queries;

namespace Hermes.Messaging.Wcf
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public abstract class QueryService
    {
        private static readonly ILog Logger = LogFactory.BuildLogger(typeof(QueryService));

        public virtual Task<TResult> Get<TQuery, TResult>(TQuery query) where TQuery : IReturn<TResult>
        {
            using (var scope = Settings.RootContainer.BeginLifetimeScope())
            {
                Logger.Verbose("Received query {0}", query.ToString());
                var queryHandler = scope.GetInstance<IAnswerQuery<TQuery, TResult>>();
                return Task.Factory.StartNew(() => queryHandler.Answer(query));
            }
        }
    }
}