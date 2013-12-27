using System;

namespace Hermes.Pipes
{
    public interface IModule<in T>
    {
        bool Invoke(T input, Func<bool> next);
    }
}