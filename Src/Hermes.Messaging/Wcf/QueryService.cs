using System.ServiceModel;
using System.Threading.Tasks;

using Hermes.Messaging.Configuration;

namespace Hermes.Messaging.Wcf
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public abstract class QueryService<TQuery, TResult>
        : IQueryService<TQuery, TResult> where TQuery : IReturn<TResult>
    {
        public virtual Task<TResult> Query(TQuery query)
        {
            using (var scope = Settings.RootContainer.BeginLifetimeScope())
            {
                var queryHandler = scope.GetInstance<IAnswerQuery<TQuery, TResult>>();
                return Task.Factory.StartNew(() => queryHandler.Answer(query));
            }
        }
    }
}