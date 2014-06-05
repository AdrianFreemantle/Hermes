using System;
using Hermes.Scheduling;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;

namespace Hermes.Cron.Tests
{
    [TestClass]
    public class CronTests
    {
        [TestMethod, ExpectedException(typeof(ArgumentNullException))]
        public void Null_expressions_are_not_allowed()
        {
            Scheduling.Cron.Parse(null);
        }

        [TestMethod, ExpectedException(typeof(ArgumentException))]
        public void Empty_expressions_are_not_allowed()
        {
            Scheduling.Cron.Parse(string.Empty);
        }

        [TestMethod, ExpectedException(typeof(CronException))]
        public void An_expression_may_not_contain_less_than_six_fields()
        {
            const string fiveFields = "1 2 3 4 5";
            Scheduling.Cron.Parse(fiveFields);
        }

        [TestMethod, ExpectedException(typeof(CronException))]
        public void An_expression_may_not_contain_more_than_six_fields()
        {
            const string sevenFields = "1 2 3 4 5 6 7";
            Scheduling.Cron.Parse(sevenFields);
        }

        [TestMethod]
        public void An_all_time_expression_returns_a_schedule()
        {
            Scheduling.Cron.Parse("* * * * * *").ShouldNotBe(null);
        }

        [TestMethod, ExpectedException(typeof(CronException))]
        public void An_expression_may_not_contain_invalid_minutes_field()
        {
            Scheduling.Cron.Parse("* bad * * * *");
        }

        [TestMethod, ExpectedException(typeof(CronException))]
        public void An_expression_may_not_contain_invalid_hours_field()
        {
            Scheduling.Cron.Parse("* * bad * * *");
        }

        [TestMethod, ExpectedException(typeof(CronException))]
        public void An_expression_may_not_contain_invalid_day_field()
        {
            Scheduling.Cron.Parse("* * * bad * *");
        }

        [TestMethod, ExpectedException(typeof(CronException))]
        public void An_expression_may_not_contain_invalid_month_field()
        {
            Scheduling.Cron.Parse("* * * * bad *");
        }

        [TestMethod, ExpectedException(typeof(CronException))]
        public void An_expression_may_not_contain_invalid_day_of_week_field()
        {
            Scheduling.Cron.Parse("* * * * * mon,bad,wed");
        }

        [TestMethod, ExpectedException(typeof(CronException))]
        public void An_expression_may_not_contain_invalid_out_of_range_value_in_field()
        {
            Scheduling.Cron.Parse("* * 1,2,3,456,7,8,9 * * *");
        }

        [TestMethod, ExpectedException(typeof(CronException))]
        public void An_expression_may_not_contain_non_number_value_in_numeric_only_field()
        {
            Scheduling.Cron.Parse("* * 1,Z,3,4 * * *");
        }

        [TestMethod, ExpectedException(typeof(CronException))]
        public void An_expression_may_not_contain_non_numeric_field_interval()
        {
            Scheduling.Cron.Parse("* * 1/Z * * *");
        }

        [TestMethod, ExpectedException(typeof(CronException))]
        public void An_expression_may_not_contain_non_numeric_field_range_component()
        {
            Scheduling.Cron.Parse("* * 3-Z2 * * *");
        }

        [TestMethod, ExpectedException(typeof(CronException))]
        public void A_range_expression_may_not_contain_extra_special_characters()
        {
            Scheduling.Cron.Parse("* * 3--2 * * *");
        }

        [TestMethod, ExpectedException(typeof(CronException))]
        public void An_expression_may_not_contain_extra_match_all_character()
        {
            Scheduling.Cron.Parse("* * * ** * *");
        }

        [TestMethod, ExpectedException(typeof(CronException))]
        public void A_field_expression_may_not_start_with_an_increment_seperator_character()
        {
            Scheduling.Cron.Parse("* * * /1 * *");
        }

        [TestMethod, ExpectedException(typeof(CronException))]
        public void A_field_expression_may_not_end_with_an_increment_seperator_character()
        {
            Scheduling.Cron.Parse("* * * 1/ * *");
        }

        [TestMethod, ExpectedException(typeof(CronException))]
        public void A_field_expression_may_not_only_contain_a_increment_speperator_character()
        {
            Scheduling.Cron.Parse("* * * / * *");
        }

        [TestMethod, ExpectedException(typeof(CronException))]
        public void A_field_expression_may_not_start_with_a_range_seperator_character()
        {
            Scheduling.Cron.Parse("* * * -1 * *");
        }

        [TestMethod, ExpectedException(typeof(CronException))]
        public void A_field_expression_may_not_end_with_a_range_seperator_character()
        {
            Scheduling.Cron.Parse("* * * 1- * *");
        }

        [TestMethod, ExpectedException(typeof(CronException))]
        public void A_field_expression_may_not_only_contain_a_range_speperator_character()
        {
            Scheduling.Cron.Parse("* * * - * *");
        }

        [TestMethod, ExpectedException(typeof(CronException))]
        public void A_field_expression_may_not_end_with_an_all_values_character()
        {
            Scheduling.Cron.Parse("* * * 1/* * *");
        }
    }
}