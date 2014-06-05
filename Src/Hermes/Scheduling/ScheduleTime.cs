using System;
using System.Globalization;

namespace Hermes.Scheduling
{
    internal class ScheduleTime 
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public int Day { get; set; }
        public int Hour { get; set; }
        public int Minute { get; set; }
        public int Second { get; set; }

        //Named constructor that reveals the "next minute" behaviour
        public ScheduleTime(DateTime baseTime)
        {
            Year = baseTime.Year;
            Month = baseTime.Month;
            Day = baseTime.Day;
            Hour = baseTime.Hour;
            Minute = baseTime.Minute;
            Second = baseTime.Second;
        }

        public ScheduleTime(int year, int month, int day)
        {
            Year = year;
            Month = month;
            Day = day;
            Hour = 0;
            Minute = 0;
            Second = 0;
        }

        public DateTime ToDateTime()
        {
            try
            {
                return new DateTime(Year, Month, Day, Hour, Minute, Second, CultureInfo.InvariantCulture.Calendar);
            }
            catch (Exception ex)
            {
                throw new CronException(String.Format(ErrorMessages.DateTimeFormatError, this), ex);
            }
        }

        public override string ToString()
        {
            return String.Format("{0:0000}/{1:00}/{2:00} {3:00}:{4:00}:{5:00})", Year, Month, Day, Hour, Minute, Second);
        }
    }
}