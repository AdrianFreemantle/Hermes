using System;
using System.Collections.Generic;

namespace Hermes.Equality
{
    /// <summary>
    /// For use in Linq queries 
    /// </summary>
    public class TypeEqualityComparer : IEqualityComparer<Type>
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