using System;

namespace Hermes.Pipes
{
    public interface IModule<in T>
    {
        bool ExtractMessage(T input, Func<bool> next);
    }
}