using System;

namespace Hermes.Persistence
{
    public interface ISequentialGuidId
    {
        Guid Id { get; set; }
    }
}