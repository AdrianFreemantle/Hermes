namespace Hermes.Queries
{
    public interface IAnswerQuery<in TQuery, out TResult> where TQuery : IReturn<TResult>
    {
        TResult Answer(TQuery query);
    }
}