using System;
using System.Globalization;
using Hermes.CronSchedule.Fields;

namespace Hermes.CronSchedule
{
    public class CronSchedule
    {
        const int MaxDayInMonthFixAttempts = 12 * 10; //12 months * 10 ; means we check if the date can be found up to 10 years in the future.

        internal MinuteField MinuteField { get; private set; }
        internal HourField HourField { get; private set; }
        internal DayOfMonthField DayOfMonthField { get; private set; }
        internal MonthField MonthField { get; private set; }
        internal DayOfWeekField DayOfWeek { get; private set; }

        private static Calendar Calendar
        {
            get { return CultureInfo.InvariantCulture.Calendar; }
        }

        internal CronSchedule(MinuteField minuteField, HourField hourField, DayOfMonthField dayOfMonthField, MonthField monthField, DayOfWeekField dayOfWeek)
        {
            MinuteField = minuteField;
            HourField = hourField;
            DayOfMonthField = dayOfMonthField;
            MonthField = monthField;
            DayOfWeek = dayOfWeek;
        }

        public DateTime GetNextOccurrence(DateTime baseTime)
        {
            var nextScheduleTime = new ScheduleTime(baseTime.AddMinutes(1));

            return GetNextOccurrence(nextScheduleTime);
        }

        private DateTime GetNextOccurrence(ScheduleTime nextScheduleTime)
        {
            CalculateNextScheduledMinute(nextScheduleTime);
            CalculateNextScheduledHour(nextScheduleTime);
            CalculateNextScheduledDay(nextScheduleTime);
            CalculateNextScheduledMonth(nextScheduleTime);

            var nextTime = TryCorrectScheduleForDaysInMonth(nextScheduleTime);
            return CorrectScheduleForDayOfWeek(nextTime);
        }

        private DateTime CorrectScheduleForDayOfWeek(DateTime nextTime)
        {
            if (DayOfWeek.Contains((int)nextTime.DayOfWeek))
            {
                return nextTime;
            }

            return GetNextOccurrence(new ScheduleTime(nextTime.Year, nextTime.Month, nextTime.Day + 1)); //recursive call
        }

        private DateTime TryCorrectScheduleForDaysInMonth(ScheduleTime nextScheduleTime)
        {
            try
            {
                return CorrectScheduleForDaysInMonth(nextScheduleTime);
            }
            catch (ArgumentOutOfRangeException ex)
            {
                throw new CronException(ErrorMessages.NoScheduleFound, ex);
            }
        }

        private DateTime CorrectScheduleForDaysInMonth(ScheduleTime nextScheduleTime)
        {
            for (int i = 0; i <= MaxDayInMonthFixAttempts; i++)
            {
                if (MonthContainsDate(nextScheduleTime))
                {
                    return nextScheduleTime.ToDateTime();
                }

                WrapToNextMonth(nextScheduleTime);
                CalculateNextScheduledMonth(nextScheduleTime);
            }

            throw new CronException(ErrorMessages.NoScheduleFound);
        }

        private static bool MonthContainsDate(ScheduleTime nextScheduleTime)
        {
            return nextScheduleTime.Day <= Calendar.GetDaysInMonth(nextScheduleTime.Year, nextScheduleTime.Month);
        }

        private void CalculateNextScheduledMonth(ScheduleTime next)
        {
            CronValue scheduledMonth = MonthField.GetNext(next.Month);

            if (scheduledMonth == CronValue.Wrapped)
            {
                WrapToNextYear(next);
            }
            else if (scheduledMonth.Value > next.Month)
            {
                SetStartOfMonth(next, scheduledMonth);
            }
            else
            {
                next.Month = scheduledMonth.Value;
            }
        }

        private void SetStartOfMonth(ScheduleTime next, CronValue scheduledMonth)
        {
            next.Month = scheduledMonth.Value;
            next.Day = DayOfMonthField.GetFirst().Value;
            next.Hour = HourField.GetFirst().Value;
            next.Minute = MinuteField.GetFirst().Value;
        }

        private void WrapToNextYear(ScheduleTime next)
        {
            next.Year++;
            next.Month = MonthField.GetFirst().Value;
            next.Day = DayOfMonthField.GetFirst().Value;
            next.Hour = HourField.GetFirst().Value;
            next.Minute = MinuteField.GetFirst().Value;
        }

        private void CalculateNextScheduledDay(ScheduleTime next)
        {
            CronValue scheduledDay = DayOfMonthField.GetNext(next.Day);

            if (scheduledDay == CronValue.Wrapped)
            {
                WrapToNextMonth(next);
            }
            else if (scheduledDay.Value > next.Day)
            {
                SetStartOfDay(next, scheduledDay);
            }
            else
            {
                next.Day = scheduledDay.Value;
            }
        }

        private void SetStartOfDay(ScheduleTime next, CronValue scheduledDay)
        {
            next.Day = scheduledDay.Value;
            next.Hour = HourField.GetFirst().Value;
            next.Minute = MinuteField.GetFirst().Value;
        }

        private void WrapToNextMonth(ScheduleTime next)
        {
            next.Month++;
            next.Day = DayOfMonthField.GetFirst().Value;
            next.Hour = HourField.GetFirst().Value;
            next.Minute = MinuteField.GetFirst().Value;
        }

        private void CalculateNextScheduledHour(ScheduleTime next)
        {
            CronValue scheduledHour = HourField.GetNext(next.Hour);

            if (scheduledHour == CronValue.Wrapped)
            {
                WrapToNextDay(next);
            }
            else if (scheduledHour.Value > next.Hour)
            {
                SetStartOfHour(next, scheduledHour);
            }
            else
            {
                next.Hour = scheduledHour.Value;
            }
        }

        private void SetStartOfHour(ScheduleTime next, CronValue scheduledHour)
        {
            next.Hour = scheduledHour.Value;
            next.Minute = MinuteField.GetFirst().Value;
        }

        private void WrapToNextDay(ScheduleTime next)
        {
            next.Day++;
            next.Hour = HourField.GetFirst().Value;
            next.Minute = MinuteField.GetFirst().Value;
        }

        private void CalculateNextScheduledMinute(ScheduleTime next)
        {
            CronValue scheduledMinute = MinuteField.GetNext(next.Minute);

            if (scheduledMinute == CronValue.Wrapped)
            {
                WrapToNextHour(next);
            }
            else
            {
                next.Minute = scheduledMinute.Value;
            }
        }

        private void WrapToNextHour(ScheduleTime next)
        {
            next.Hour++;
            next.Minute = MinuteField.GetFirst().Value;
        }
    }
}
