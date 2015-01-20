using System.Collections.Generic;

namespace Hermes.Pipes
{
    public interface IFilter<T>
    {
        ICollection<T> Filter(ICollection<T> input);
    }
}