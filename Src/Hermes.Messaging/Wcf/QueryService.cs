using System.ServiceModel;
using System.Threading.Tasks;

using Hermes.Messaging.Configuration;

namespace Hermes.Messaging.Wcf
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public abstract class QueryService
    {
        public virtual Task<TResult> Get<TQuery, TResult>(TQuery query) where TQuery : IReturn<TResult>
        {
            using (var scope = Settings.RootContainer.BeginLifetimeScope())
            {
                var queryHandler = scope.GetInstance<IAnswerQuery<TQuery, TResult>>();
                return Task.Factory.StartNew(() => queryHandler.Answer(query));
            }
        }
    }
}