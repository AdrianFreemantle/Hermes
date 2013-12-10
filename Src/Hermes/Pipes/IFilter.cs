using System.Collections.Generic;

namespace Hermes.Pipes
{
    public interface IFilter<T>
    {
        IEnumerable<T> Filter(IEnumerable<T> input);
    }
}