using System.Collections.Generic;

namespace Hermes.Pipes
{
    public interface IOperation<T>
    {
        IEnumerable<T> Execute(IEnumerable<T> input);
    }
}