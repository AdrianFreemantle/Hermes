﻿namespace Hermes
{
    public interface IInMemoryBus
    {
        void Raise(object @event);
        void Execute(object command);
    }
}