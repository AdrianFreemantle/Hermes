using System;
using Hermes.Scheduling;
using Hermes.Scheduling.Fields;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;

namespace Hermes.Cron.Tests
{
    [TestClass]
    public class CronFieldTests
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Throw_ArgumentNullException_on_null_expressions()
        {
            var field = new MinuteField(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Throw_ArgumentException_on_empty_expressions()
        {
            var field = new MinuteField(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(CronException))]
        public void Throw_CronException_on_expression_containing_invalid_characters()
        {
            var field = new MinuteField("*/15+");
        }

        [TestMethod]
        public void Field_can_accept_match_all_values_expression()
        {
            var field = new MinuteField("*");

            field.GetFirst().Value.ShouldBe(0);
            field.GetNext(30).Value.ShouldBe(30);
            field.GetNext(59).Value.ShouldBe(59);
            field.GetNext(60).Value.ShouldBe(CronValue.Wrapped.Value);
        }

        [TestMethod]
        public void Field_can_accept_a_simple_list_expression()
        {
            var field = new MinuteField("10,20");

            field.GetFirst().Value.ShouldBe(10);
            field.GetNext(10).Value.ShouldBe(10);
            field.GetNext(11).Value.ShouldBe(20);
            field.GetNext(20).Value.ShouldBe(20);
            field.GetNext(21).Value.ShouldBe(CronValue.Wrapped.Value);
        }

        [TestMethod]
        public void Field_can_accept_a_simple_range_expression()
        {
            var field = new MinuteField("10-14");

            field.GetFirst().Value.ShouldBe(10);
            field.GetNext(10).Value.ShouldBe(10);
            field.GetNext(11).Value.ShouldBe(11);
            field.GetNext(12).Value.ShouldBe(12);
            field.GetNext(13).Value.ShouldBe(13);
            field.GetNext(14).Value.ShouldBe(14);
            field.GetNext(15).Value.ShouldBe(CronValue.Wrapped.Value);
        }

        [TestMethod]
        public void Field_can_accept_a_list_of_ranges_expression()
        {
            var field = new MinuteField("10-11,20-21");

            field.GetFirst().Value.ShouldBe(10);
            field.GetNext(10).Value.ShouldBe(10);
            field.GetNext(11).Value.ShouldBe(11);
            field.GetNext(12).Value.ShouldBe(20);
            field.GetNext(20).Value.ShouldBe(20);
            field.GetNext(21).Value.ShouldBe(21);
            field.GetNext(22).Value.ShouldBe(CronValue.Wrapped.Value);
        }

        [TestMethod]
        public void Field_can_accept_a_simple_increment_expression()
        {
            var field = new MinuteField("*/10");

            field.GetFirst().Value.ShouldBe(0);
            field.GetNext(1).Value.ShouldBe(10);
            field.GetNext(10).Value.ShouldBe(10);
            field.GetNext(11).Value.ShouldBe(20);
            field.GetNext(21).Value.ShouldBe(30);
            field.GetNext(51).Value.ShouldBe(CronValue.Wrapped.Value);
        }

        [TestMethod]
        public void Field_can_accept_a_ranged_increment_expression()
        {
            var field = new MinuteField("10-40/10");

            field.GetFirst().Value.ShouldBe(10);
            field.GetNext(10).Value.ShouldBe(10);
            field.GetNext(11).Value.ShouldBe(20);
            field.GetNext(21).Value.ShouldBe(30);
            field.GetNext(31).Value.ShouldBe(40);
            field.GetNext(41).Value.ShouldBe(CronValue.Wrapped.Value);
        }
    }
}
