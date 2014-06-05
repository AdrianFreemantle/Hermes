using System;
using System.Globalization;
using Hermes.Scheduling;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Shouldly;
    //  CRON EXPRESSION
    //  ────────────────────────────────────────────────────────────────────────────────
    //  ┌────┬────┬────┬────┬────┬─ Cron Field-Expressions
    //  |    |    |    |    |    |
    //  ┴    ┴    ┴    ┴    ┴    ┴
    //  *    *    *    *    *    *  ├── Cron Expression
    //  ┬    ┬    ┬    ┬    ┬    ┬
    //  │    │    │    │    │    │
    //  │    │    │    │    │    └── day of week (0 - 6) (0 = Sunday) (Mon - Sun) (Monday - Sunday)
    //  │    │    │    │    └─────── month (1 - 12) (1 = January) (Jan - Dec) (January - December)
    //  │    │    │    └──────────── day of month (1 - 31)
    //  │    │    └───────────────── hour (0 - 23)
    //  │    └────────────────────── min (0 - 59)
    //  └─────────────────────────── sec (0 - 59) 
    //
    //  ALLOWED SPECIAL CHARACTERS
    //  ────────────────────────────────────────────────────────────────────────────────
    //  Asterisk ( * )
    //  The asterisk indicates that the cron expression will match for all values of the field.
    //  For example, using an asterisk in the 4th field (month) would indicate every month.
    //
    //  Slash ( / )
    //  Slashes are used to describe increments of ranges. 
    //  For example 3-59/15 in the 1st field (minutes) would indicate the 3rd minute of the hour and every 15 minutes thereafter. 
    //  The form "*/..." is equivalent to the form "first-last/...", that is, an increment over the largest possible range of the field.
    //  The form "*/10" in the 1st field (minutes) would indicate every 10 minutes, i.e. minutes 0, 10, 20, 30, 40 and 50.
    //
    //  Comma ( , )
    //  Commas are used to separate items of a list. 
    //  For example, using "1,3,5" in the 5th field (day of week) would mean Mondays, Wednesdays and Fridays.
    //
    //  Hyphen ( - )
    //  Hyphens are used to define ranges. 
    //  For example, 1-5 in the 5th field (day of week) would indicate every week day, Monday to Friday.
    //
    //  The above information is derived from http://en.wikipedia.org/wiki/Cron

namespace Hermes.Cron.Tests
{
    [TestClass]
    public class CronScheduleTests
    {
        [TestMethod]
        public void Every_minute()
        {
            var schedule = Scheduling.Cron.Parse("* * * * * *");

            schedule.GetNextOccurrence(Time("2003/01/01 00:01:00")).ShouldBe(Time("2003/01/01 00:02:00"));
            schedule.GetNextOccurrence(Time("2003/01/01 00:59:00")).ShouldBe(Time("2003/01/01 01:00:00"));
            schedule.GetNextOccurrence(Time("2003/01/01 23:59:00")).ShouldBe(Time("2003/01/02 00:00:00"));
            schedule.GetNextOccurrence(Time("2003/01/31 23:59:00")).ShouldBe(Time("2003/02/01 00:00:00"));
            schedule.GetNextOccurrence(Time("2003/12/31 23:59:00")).ShouldBe(Time("2004/01/01 00:00:00"));
        }

        [TestMethod]
        public void Tenth_minute_of_every_hour()
        {
            var schedule = Scheduling.Cron.Parse("0 10 * * * *");

            schedule.GetNextOccurrence(Time("2003/01/01 00:01:00")).ShouldBe(Time("2003/01/01 00:10:00"));
            schedule.GetNextOccurrence(Time("2003/01/01 00:11:00")).ShouldBe(Time("2003/01/01 01:10:00"));
            schedule.GetNextOccurrence(Time("2003/01/01 00:50:00")).ShouldBe(Time("2003/01/01 01:10:00"));
            schedule.GetNextOccurrence(Time("2003/01/01 23:51:00")).ShouldBe(Time("2003/01/02 00:10:00"));
            schedule.GetNextOccurrence(Time("2003/01/31 23:55:00")).ShouldBe(Time("2003/02/01 00:10:00"));
            schedule.GetNextOccurrence(Time("2003/12/31 23:59:00")).ShouldBe(Time("2004/01/01 00:10:00"));
        }

        [TestMethod]
        public void Every_thirty_minutes()
        {
            var schedule = Scheduling.Cron.Parse("0 */30 * * * *");

            schedule.GetNextOccurrence(Time("2003/01/01 00:00:59")).ShouldBe(Time("2003/01/01 00:30:00"));
            schedule.GetNextOccurrence(Time("2003/01/01 00:10:58")).ShouldBe(Time("2003/01/01 00:30:00"));
            schedule.GetNextOccurrence(Time("2003/01/01 00:30:57")).ShouldBe(Time("2003/01/01 01:00:00"));
            schedule.GetNextOccurrence(Time("2003/01/01 00:45:56")).ShouldBe(Time("2003/01/01 01:00:00"));
            schedule.GetNextOccurrence(Time("2003/01/01 23:59:55")).ShouldBe(Time("2003/01/02 00:00:00"));
        }

        [TestMethod]
        public void Every_ten_seconds_between_eight_and_five_on_week_days()
        {
            var schedule = Scheduling.Cron.Parse("*/10 * 8-17 * * Monday-Friday");

            schedule.GetNextOccurrence(Time("2014/06/01 00:00:00")).ShouldBe(Time("2014/06/02 08:00:00"));
            schedule.GetNextOccurrence(Time("2014/06/02 13:04:15")).ShouldBe(Time("2014/06/02 13:04:20"));
            schedule.GetNextOccurrence(Time("2014/06/02 23:59:55")).ShouldBe(Time("2014/06/03 08:00:00"));
        }

        [TestMethod]
        public void Twentieth_minute_of_the_hour_and_everty_minute_thereafter_until_the_fortieth_minute()
        {
            var schedule = Scheduling.Cron.Parse("0 20-40 * * * *");

            schedule.GetNextOccurrence(Time("2003/01/01 00:00:00")).ShouldBe(Time("2003/01/01 00:20:00"));
            schedule.GetNextOccurrence(Time("2003/01/01 00:10:00")).ShouldBe(Time("2003/01/01 00:20:00"));
            schedule.GetNextOccurrence(Time("2003/01/01 00:19:00")).ShouldBe(Time("2003/01/01 00:20:00"));
            schedule.GetNextOccurrence(Time("2003/01/01 00:20:00")).ShouldBe(Time("2003/01/01 00:21:00"));
            schedule.GetNextOccurrence(Time("2003/01/01 00:30:00")).ShouldBe(Time("2003/01/01 00:31:00"));
            schedule.GetNextOccurrence(Time("2003/01/01 00:40:00")).ShouldBe(Time("2003/01/01 01:20:00"));
            schedule.GetNextOccurrence(Time("2003/01/01 00:50:00")).ShouldBe(Time("2003/01/01 01:20:00"));
        }

        [TestMethod]
        public void Tenth_minute_of_the_hour_and_every_five_minutes_thereafter()
        {
            var schedule = Scheduling.Cron.Parse("0 10-59/5 * * * *");

            schedule.GetNextOccurrence(Time("2003/01/01 00:00:00")).ShouldBe(Time("2003/01/01 00:10:00"));
            schedule.GetNextOccurrence(Time("2003/01/01 00:08:00")).ShouldBe(Time("2003/01/01 00:10:00"));
            schedule.GetNextOccurrence(Time("2003/01/01 00:10:00")).ShouldBe(Time("2003/01/01 00:15:00"));
            schedule.GetNextOccurrence(Time("2003/01/01 00:14:00")).ShouldBe(Time("2003/01/01 00:15:00"));
            schedule.GetNextOccurrence(Time("2003/01/01 00:15:00")).ShouldBe(Time("2003/01/01 00:20:00"));
            schedule.GetNextOccurrence(Time("2003/01/01 00:20:00")).ShouldBe(Time("2003/01/01 00:25:00"));
        }

        [TestMethod]
        public void Midnight_on_first_day_of_month_if_day_is_monday()
        {
            var schedule = Scheduling.Cron.Parse("0 0 0 1 * Mon");

            schedule.GetNextOccurrence(Time("2013/01/01 00:00:00")).ShouldBe(Time("2013/04/01 00:00:00"));
        }

        [TestMethod]
        public void Eight_in_the_morning_on_the_first_monday_of_the_month()
        {
            var schedule = Scheduling.Cron.Parse("0 0 8 1,2,3,4,5,6,7 * Monday");

            schedule.GetNextOccurrence(Time("2013/01/01 00:00:00")).ShouldBe(Time("2013/01/07 08:00:00"));
            schedule.GetNextOccurrence(Time("2013/02/01 00:00:00")).ShouldBe(Time("2013/02/04 08:00:00"));
            schedule.GetNextOccurrence(Time("2013/03/01 00:00:00")).ShouldBe(Time("2013/03/04 08:00:00"));
            schedule.GetNextOccurrence(Time("2013/04/01 00:00:00")).ShouldBe(Time("2013/04/01 08:00:00"));
        }

        [TestMethod]
        public void Midnight_on_the_first_day_of_the_year()
        {
            var schedule = Scheduling.Cron.Parse("0 0 0 0 1 *");

            schedule.GetNextOccurrence(Time("2011/11/01 00:00:00")).ShouldBe(Time("2012/01/01 00:00:00"));
            schedule.GetNextOccurrence(Time("2012/10/16 00:00:00")).ShouldBe(Time("2013/01/01 00:00:00"));
            schedule.GetNextOccurrence(Time("2013/09/11 00:00:00")).ShouldBe(Time("2014/01/01 00:00:00"));
            schedule.GetNextOccurrence(Time("2014/04/21 00:00:00")).ShouldBe(Time("2015/01/01 00:00:00"));
        }

        [TestMethod]
        public void Thirtyfirst_of_the_month()
        {
            var schedule = Scheduling.Cron.Parse("0 0 0 31 * *"); //31st of every month

            schedule.GetNextOccurrence(Time("2013/01/01 00:00:00")).ShouldBe(Time("2013/01/31 00:00:00"));
            schedule.GetNextOccurrence(Time("2013/02/01 00:00:00")).ShouldBe(Time("2013/03/31 00:00:00"));
            schedule.GetNextOccurrence(Time("2013/03/01 00:00:00")).ShouldBe(Time("2013/03/31 00:00:00"));
            schedule.GetNextOccurrence(Time("2013/04/01 00:00:00")).ShouldBe(Time("2013/05/31 00:00:00"));
            schedule.GetNextOccurrence(Time("2013/05/01 00:00:00")).ShouldBe(Time("2013/05/31 00:00:00"));
            schedule.GetNextOccurrence(Time("2013/06/01 00:00:00")).ShouldBe(Time("2013/07/31 00:00:00"));
            schedule.GetNextOccurrence(Time("2013/07/01 00:00:00")).ShouldBe(Time("2013/07/31 00:00:00"));
            schedule.GetNextOccurrence(Time("2013/08/01 00:00:00")).ShouldBe(Time("2013/08/31 00:00:00"));
            schedule.GetNextOccurrence(Time("2013/09/01 00:00:00")).ShouldBe(Time("2013/10/31 00:00:00"));
            schedule.GetNextOccurrence(Time("2013/10/01 00:00:00")).ShouldBe(Time("2013/10/31 00:00:00"));
            schedule.GetNextOccurrence(Time("2013/11/01 00:00:00")).ShouldBe(Time("2013/12/31 00:00:00"));
            schedule.GetNextOccurrence(Time("2013/12/01 00:00:00")).ShouldBe(Time("2013/12/31 00:00:00"));
        }

        [TestMethod, ExpectedException(typeof(CronException))]
        public void Do_not_loop_infinitely_if_day_of_week_cant_be_found()
        {
            var schedule = Scheduling.Cron.Parse("* * * 31 February Monday");

            schedule.GetNextOccurrence(Time("2011/11/01 00:00:00"));
        }

        [TestMethod, ExpectedException(typeof(CronException))]
        public void Do_not_loop_infinitely_if_date_cant_be_found()
        {
            var schedule = Scheduling.Cron.Parse("* * * 31 Feb Monday");

            schedule.GetNextOccurrence(Time("2011/11/01 00:00:00"));
        }

        private static DateTime Time(string str)
        {
            return DateTime.ParseExact(str, "yyyy/MM/dd HH:mm:ss", CultureInfo.InvariantCulture);
        }
    }
}