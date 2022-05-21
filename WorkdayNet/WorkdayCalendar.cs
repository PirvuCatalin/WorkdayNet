using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkdayNet
{
    class WorkDay
    {
        public int StartHours { get; set; }
        public int StartMinutes { get; set; }

        public int StopHours { get; set; }

        public int StopMinutes { get; set; }
    }

    class RecurringHoliday
    {
        public int month { get; set; }
        public int day { get; set; }
    }

    public class WorkdayCalendar : IWorkdayCalendar
    {
        private WorkDay workDay { get; set; }

        List<DateTime> holidays = new List<DateTime>();
        List<RecurringHoliday> recurringHolidays = new List<RecurringHoliday>();

        public void SetHoliday(DateTime date)
        {
            holidays.Add(date);
        }

        public void SetRecurringHoliday(int month, int day)
        {
            RecurringHoliday recurringHoliday = new RecurringHoliday();
            recurringHoliday.month = month;
            recurringHoliday.day = day;
            recurringHolidays.Add(recurringHoliday);
        }

        public void SetWorkdayStartAndStop(int startHours, int startMinutes, int stopHours, int stopMinutes)
        {
            workDay = new WorkDay();
            workDay.StartHours = startHours;
            workDay.StartMinutes = startMinutes;
            workDay.StopHours = stopHours;
            workDay.StopMinutes = stopMinutes;
        }

        public DateTime GetWorkdayIncrement(DateTime startDate, decimal incrementInWorkdays)
        {
            bool revert = false;
            if(incrementInWorkdays < 0) {
                revert = true;
            }
            // the commented-out implementation below is for the simple scenario when having 
            // workday job that can be done in the same day
            //TimeSpan timeSpan = ConvertIncrementToTimeSpan(incrementInWorkdays);
            //return startDate.Add(timeSpan);

            // clever thing here is that full workdays can be easily added directly to the startDate
            // this works because in a period of 24 hours you're going to work a "workday",
            // which can be any value between 0 and 24 hours (ofc usually 8 hours)

            TimeSpan timeSpan = ConvertIncrementToTimeSpan(incrementInWorkdays);

            //if (timeSpan.Days > 0)
            //{
            //    // clever thing here is that full workdays can be easily added directly to the startDate
            //    // this works because in a period of 24 hours you're going to work a "workday",
            //    // which can be any value between 0 and 24 hours (ofc usually 8 hours)
            //    startDate.AddDays(timeSpan.Days);
            //    timeSpan = new TimeSpan(0, timeSpan.Hours, timeSpan.Minutes, 0);
            //}

            startDate = CheckHoliday(startDate, true, revert);

            // checks if startDate is outside business hours
            if (IsOutsideBusinessHours(startDate, startDate, 0))
            {
                if (revert)
                {
                    startDate = new DateTime(startDate.Year, startDate.Month, startDate.Day, workDay.StopHours, workDay.StopMinutes, 0);
                }
                else
                {
                    startDate = startDate.AddDays(1);
                    startDate = new DateTime(startDate.Year, startDate.Month, startDate.Day, workDay.StartHours, workDay.StartMinutes, 0);
                }
            }
            else if (IsStartDateBeforeStartHours(startDate))
            {
                if (revert)
                {
                    startDate = startDate.AddDays(-1);
                    startDate = new DateTime(startDate.Year, startDate.Month, startDate.Day, workDay.StopHours, workDay.StopMinutes, 0);
                } else
                {
                    startDate = new DateTime(startDate.Year, startDate.Month, startDate.Day, workDay.StartHours, workDay.StartMinutes, 0);
                }
            }

            startDate = CheckHoliday(startDate, false, revert);

            if (revert)
            {
                for (int i = 0; i < -timeSpan.Days; i++)
                {
                    startDate = startDate.AddDays(-1);

                    startDate = CheckHoliday(startDate, false, revert);
                }
            } else
            {
                for (int i = 0; i < timeSpan.Days; i++)
                {
                    startDate = startDate.AddDays(1);

                    startDate = CheckHoliday(startDate, false, revert);
                }
            }
                // add days one by one...
                

            timeSpan = new TimeSpan(0, timeSpan.Hours, timeSpan.Minutes, 0);

            DateTime endDate = startDate.Add(timeSpan);

            if (IsOutsideBusinessHours(endDate, startDate, timeSpan.Days))
            {
                // let's stop for today, let's do it tomorrow
                TimeSpan extraHours = GetExtraWorkedHours(endDate);
                endDate = endDate.AddDays(1);

                endDate = new DateTime(endDate.Year, endDate.Month, endDate.Day, workDay.StartHours, workDay.StartMinutes, 0);
                endDate = CheckHoliday(endDate, false, revert);

                // we're sure here that this won't lead to outside business hours as it's less than a workday
                endDate = endDate.AddHours(extraHours.Hours);
                endDate = endDate.AddMinutes(extraHours.Minutes);
            }

            return endDate;
        }

        // the idea is that this should be called everytime we add one workday to the enddate
        // but can also be used to ensure that startdate is at the first working day (not weekend, not holiday)
        // 
        // todo: could be implemented better to not be called manually everytime, but as part of "DateTime.AddDays()" method
        public DateTime CheckHoliday(DateTime date, bool isStartDate, bool revert)
        {
            bool leapDay = false;

            // weekend
            // holidays
            while (date.DayOfWeek == DayOfWeek.Saturday ||
                date.DayOfWeek == DayOfWeek.Sunday ||
                holidays.Any(holiday => 
                    (holiday.Year == date.Year && holiday.Month == date.Month && holiday.Day == date.Day)) ||
                recurringHolidays.Any(holiday =>
                    (holiday.month == date.Month && holiday.day == date.Day)))
            {
                if (revert)
                {
                    date = date.AddDays(-1);
                } else
                {
                    date = date.AddDays(1);
                }
                
                leapDay = true;
            }

            if (isStartDate && leapDay)
            {
                if (revert)
                {
                    date = new DateTime(date.Year, date.Month, date.Day, workDay.StopHours, workDay.StopMinutes, 0);
                } 
                else
                {
                    // isStartDate flag also sets the start hours / minutes, as it's starting in a non working day
                    date = new DateTime(date.Year, date.Month, date.Day, workDay.StartHours, workDay.StartMinutes, 0);
                }
            }

            return date;
        }

        private TimeSpan GetExtraWorkedHours(DateTime endDate)
        {
            DateTime actualEndDate = new DateTime(endDate.Year, endDate.Month, endDate.Day, workDay.StopHours, workDay.StopMinutes, endDate.Second);

            if (endDate.Hour < workDay.StopHours)
            {
                actualEndDate = actualEndDate.AddDays(-1); // we leaped a day
            }

            return endDate - actualEndDate; // where we only care about hours and minutes
        }

        private bool IsOutsideBusinessHours(DateTime endDate, DateTime startDate, int expectedDays)
        {
            if ((endDate - startDate).Days != expectedDays)
            {
                // this is one special case where worked hours in a day could lead to leaping a day
                return true;
            }

            if (workDay.StopHours < workDay.StartHours) // if work takes place during day leaps
            {
                if (endDate.Hour >= workDay.StartHours)
                {
                    return false;
                }

                if (endDate.Hour < workDay.StopHours)
                {
                    return false;
                }

                if (endDate.Hour == workDay.StopHours &&
                    endDate.Minute <= workDay.StopMinutes)
                {
                    return false;
                }

                return true;
            }

            if (endDate.Hour > workDay.StopHours)
            {
                return true;
            }

            if (endDate.Hour == workDay.StopHours &&
                endDate.Minute > workDay.StopMinutes)
            {
                return true;
            }

            return false;
        }

        public bool IsStartDateBeforeStartHours(DateTime startDate)
        {
            if (startDate.Hour < workDay.StartHours)
            {
                return true;
            }

            if (startDate.Hour == workDay.StartHours &&
                startDate.Minute < workDay.StartMinutes)
            {
                return true;
            }

            return false;
        }

        // the actual increment is decimal workdays and the expected precision is minutes,
        // so we can discard seconds -> miliseconds ...
        // e.g. for 24-05-2004 19:03 with an addition of 44.723656 work days is 27-07-2004 13:47
        // we can just focus on the hours + minutes part 
        //      0.723656 * 8h = 5.789248 h => that means we're going to reach 13:00 ->
        //      0.789248 * 60m = 47.35488 m => that means the final time is 13:47
        //      we could go further and see that 0.35488 * 60s = 21.2928 s => the final time is actually 13:47:21
        // but we won't
        //
        // TODO: make this also work with seconds, milisecond precision (time = money)
        //
        //  Important note: TimeSpan provides helpful methods (FromDays),
        //  but in our case the workdays = variable hours so we'll just use it for passing data
        public TimeSpan ConvertIncrementToTimeSpan(decimal incrementInWorkdays)
        {
            // would be integer, but can overflow
            decimal days = Math.Truncate(incrementInWorkdays);

            decimal hoursIncrement = (incrementInWorkdays - days) * GetWorkdayInHours();
            decimal hours = Math.Truncate(hoursIncrement);

            decimal minutesIncrement = (hoursIncrement - hours) * 60;
            decimal minutes = Math.Truncate(minutesIncrement);

            // just skip seconds i.e. 0
            return new TimeSpan((int)days, (int)hours, (int)minutes, 0);
        }

        public decimal GetWorkdayInHours()
        {
            // note that this may introduce rounding issues from TimeSpan's TotalHours, but it's a good start
            // take into account minutes and also hour could be next day... so that:


            DateTime start = new DateTime(1, 1, 1, workDay.StartHours, workDay.StartMinutes, 0);
            DateTime end = new DateTime(1, 1, 1, workDay.StopHours, workDay.StopMinutes, 0);

            if (workDay.StopHours < workDay.StartHours)
            {
                end = end.AddDays(1);
            }

            TimeSpan difference = end - start;

            return (decimal)difference.TotalHours;
        }
    }
}
