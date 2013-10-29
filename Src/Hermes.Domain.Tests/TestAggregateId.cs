using System;
using System.Diagnostics;

namespace Hermes.Domain.Tests
{
    [DebuggerStepThrough]
    public class TestAggregateId : Identity<Guid>
    {
        public TestAggregateId(Guid id)
            : base(id)
        {
            
        }
    }
}