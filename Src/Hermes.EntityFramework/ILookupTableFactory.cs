using System;
using System.Linq;
using System.Runtime.Remoting.Contexts;

namespace Hermes.EntityFramework
{
    public interface ILookupTableFactory : IDisposable
    {
        void Add<TWrapper, TEnum>()
            where TWrapper : EnumWrapper<TEnum>
            where TEnum : struct, IComparable, IFormattable, IConvertible;

        void Add<TLookup>(TLookup lookup) where TLookup : class, ILookupTable;
    }
}