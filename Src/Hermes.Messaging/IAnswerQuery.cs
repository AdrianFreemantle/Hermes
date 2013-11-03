﻿namespace Hermes.Messaging
{
    public interface IAnswerQuery<in TQuery, out TResult> where TQuery : IReturn<TResult>
    {
        TResult Answer(TQuery query);
    }
}