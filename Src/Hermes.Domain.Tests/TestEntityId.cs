using System.Diagnostics;

namespace Hermes.Domain.Tests
{
    [DebuggerStepThrough]
    public class TestEntityId : Identity<int>
    {
        public TestEntityId(int id)
            : base(id)
        {

        }
    }
}