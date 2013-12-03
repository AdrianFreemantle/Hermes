using System.Collections.Generic;

namespace Hermes.Pipeline
{
    public interface IOperation<T>
    {
        IEnumerable<T> Execute(IEnumerable<T> input);
    }
}