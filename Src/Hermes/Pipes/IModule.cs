using System;
using System.Threading.Tasks;

namespace Hermes.Pipes
{
    public interface IModule<in T>
    {
        bool Process(T input, Func<bool> next);
    }
}