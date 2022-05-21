﻿using System;
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

    public class WorkdayCalendar : IWorkdayCalendar
    {
        private WorkDay workDay { get; set; }

        public void SetHoliday(DateTime date)
        {

        }

        public void SetRecurringHoliday(int month, int day)
        {

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
            TimeSpan timeSpan = IncrementToTimeSpan(incrementInWorkdays);
            return startDate.Add(timeSpan);
        }

        // the actual increment is decimal workdays and the expected precision is minutes, so we can discard seconds -> miliseconds ...
        // e.g. for 24-05-2004 19:03 with an addition of 44.723656 work days is 27-07-2004 13:47
        // we can just focus on the hours + minutes part 
        //      0.723656 * 8h = 5.789248 h => that means we're going to reach 13:00 ->
        //      0.789248 * 60m = 47.35488 m => that means the final time is 13:47
        //      we could go further and see that 0.35488 * 60s = 21.2928 s => the final time is actually 13:47:21
        // but we won't
        //
        //  Important note: TimeSpan provides helpful methods (FromDays), but in our case the workdays = variable hours so we'll just use it for passing data
        public TimeSpan IncrementToTimeSpan(decimal incrementInWorkdays)
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
