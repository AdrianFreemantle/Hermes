using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Shouldly;

namespace Hermes.Core.Tests
{
    // ReSharper disable InconsistentNaming
    [TestClass]
    public class When_retrieving_enum_descriptions
    {
        [TestMethod]
        public void A_dictionary_of_all_descriptions_for_each_value_can_be_retrieved()
        {
            var result = EnumConverter.GetAllDescriptions<DescriptionTestEnum>();

            foreach (var conversion in result)
            {
                conversion.Value.ShouldBe(conversion.Key.GetDescription());
            }
        }
    }
    // ReSharper restore InconsistentNaming

}
