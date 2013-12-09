using System;

namespace Hermes.Pipes
{
    public interface IModule<in T>
    {
        void Invoke(T input, Action next);
    }
}