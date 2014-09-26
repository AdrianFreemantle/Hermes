using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Shouldly;

namespace Hermes.Core.Tests
{
    [TestClass]
    public class StringExtensionTests
    {
        [TestMethod]
        public void GetDigitsFromString()
        {
            var mixedString = "abcde12345#%@";

            var digits = mixedString.GetAllDigits();

            digits.ShouldBe("12345");
        }
    }
}
