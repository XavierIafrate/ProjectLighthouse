using ProjectLighthouse.ViewModel.Helpers;
using RestSharp.Extensions;
using System;
using System.Collections.Generic;

namespace ProjectLighthouse.Model.Core
{
    public class OpeningHours
    {
        public Dictionary<DayOfWeek, Day> Data = new();

        public bool IsOutOfHours(DateTime date)
        {
            Day day = Data[date.DayOfWeek];

            if (!day.OpensOnDay) return true;

            DateTime openingTime = new(date.Ticks);
            openingTime = openingTime.ChangeTime(day.OpeningHour, day.OpeningMinute, 0, 0);

            if(date < openingTime) return true;

            // you don't have to go home but you can't stay here
            DateTime closingTime = new(date.Ticks);
            closingTime = closingTime.ChangeTime(day.ClosingHour, day.ClosingMinute, 0, 0);

            if (date > closingTime) return true;

            return false;
        }

        public class Day
        {
            public bool OpensOnDay = true;

            public int OpeningHour = 0;
            public int OpeningMinute = 0;

            public int ClosingHour = 23;
            public int ClosingMinute = 59;

            public TimeSpan GetOpeningHoursTimeSpan()
            {
                DateTime open = DateTime.MinValue;
                open = open.ChangeTime(this.OpeningHour, this.OpeningMinute, 0, 0);

                DateTime close = DateTime.MinValue;
                close = close.ChangeTime(this.ClosingHour, this.ClosingMinute, 0, 0);

                return close - open;
            }
        }
    }
}
