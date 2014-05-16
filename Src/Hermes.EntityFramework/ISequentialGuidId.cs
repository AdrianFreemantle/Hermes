using System;

namespace Hermes.EntityFramework
{
    public interface ISequentialGuidId
    {
        Guid Id { get; set; }
    }
}