using System;
using System.Collections.Generic;

namespace Hermes.Reflection
{
    /// <summary>
    /// For use in Linq queries 
    /// </summary>
    public class TypeComparer : IEqualityComparer<Type>
    {
        public bool Equals(Type x, Type y)
        {
            return x.AssemblyQualifiedName == y.AssemblyQualifiedName;
        }

        public int GetHashCode(Type obj)
        {
            return 0;
        }
    }
}