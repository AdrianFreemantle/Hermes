using System;

namespace Hermes.Queries
{
    [Obsolete("This will be made obsolete on 1 October 2014")]
    public interface IAnswerQuery<in TQuery, out TResult> where TQuery : IReturn<TResult>
    {
        TResult Answer(TQuery query);
    }
}