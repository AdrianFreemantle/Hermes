using System.ComponentModel;

namespace Hermes.Core.Tests
{
    public enum DescriptionTestEnum
    {
        Unknown,
        [Description("Some Long Name")]
        SomeLongName,
        AnotherLongName
    }
}