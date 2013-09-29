using Clientele.Core.Domain;

namespace Clientele.Core
{
    public interface IMemento
    {
        IHaveIdentity Identity { get; set; }
    }
}