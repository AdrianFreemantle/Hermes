using System.Collections.Generic;
using System.Linq;

namespace Hermes.Pipeline
{
    public abstract class Pipeline<T>
    {
        private readonly List<IOperation<T>> filters = new List<IOperation<T>>();

        public Pipeline<T> Register(IOperation<T> operation)
        {
            filters.Add(operation);
            return this;
        }

        public virtual IEnumerable<T> Execute(IEnumerable<T> collection)
        {
            T[] current = collection.ToArray();

            foreach (IOperation<T> filer in filters)
            {
                current = filer.Execute(current).ToArray();
            }

            return current;
        }
    }
}
