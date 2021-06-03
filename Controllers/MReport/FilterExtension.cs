using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FarroAPI.Entities;
using FarroAPI.Models;

namespace FarroAPI.Controllers
{
    public static class FilterExtension
    {
        // Set date selecting shorcut - today
        public static IFilter SetToday(this IFilter filter)
        {
            filter.StartDateTime = DateTime.Now.Date;
            filter.EndDateTime = DateTime.Now.Date.AddDays(1);

            return filter;
        }

        // Set date selecting shorcut - yesterday
        public static IFilter SetYesterday(this IFilter filter)
        {
            filter.StartDateTime = DateTime.Now.Date.AddDays(-1);
            filter.EndDateTime = DateTime.Now.Date;

            return filter;
        }

        // Set date selecting shorcut - the day before yesterday
        public static IFilter SetTheDayBeforeYesterday(this IFilter filter)
        {
            filter.StartDateTime = DateTime.Now.Date.AddDays(-2);
            filter.StartDateTime = DateTime.Now.Date.AddDays(-1);

            return filter;
        }

        // Set date selecting shorcut - this week
        public static IFilter SetThisWeek(this IFilter filter)
        {
            filter.StartDateTime = DateTime.Now.Date.AddDays(-(int)DateTime.Now.DayOfWeek + 1);
            filter.EndDateTime = DateTime.Now.Date.AddDays(1);

            return filter;
        }

        // Set date selecting shorcut - last week
        public static IFilter SetLastWeek(this IFilter filter)
        {
            filter.StartDateTime = DateTime.Now.Date.AddDays(-(int)DateTime.Now.DayOfWeek - 6);
            filter.EndDateTime = DateTime.Now.Date.AddDays(-(int)DateTime.Now.DayOfWeek + 1);

            return filter;
        }

        // Set date selecting shorcut - this month
        public static IFilter SetThisMonth(this IFilter filter)
        {
            filter.StartDateTime = DateTime.Now.Date.AddDays(-(int)DateTime.Now.Day + 1);
            filter.EndDateTime = DateTime.Now.Date.AddDays(1);

            return filter;
        }

        // Set date selecting shorcut - last month
        public static IFilter SetLastMonth(this IFilter filter)
        {
            filter.StartDateTime = DateTime.Now.Date.AddDays(-(int)DateTime.Now.Day + 1).AddMonths(-1);
            filter.EndDateTime = DateTime.Now.Date.AddDays(-(int)DateTime.Now.Day + 1);

            return filter;
        }

        // Set date selecting shorcut - last three month
        public static IFilter SetLastThreeMonths(this IFilter filter)
        {
            filter.StartDateTime = DateTime.Now.Date.AddDays(-(int)DateTime.Now.Day + 1).AddMonths(-3);
            filter.EndDateTime = DateTime.Now.Date.AddDays(-(int)DateTime.Now.Day + 1);

            return filter;
        }

        // Set date selecting shorcut - this year
        public static IFilter SetThisYear(this IFilter filter)
        {
            filter.StartDateTime = new DateTime(DateTime.Now.Year, 1, 1);
            filter.EndDateTime = DateTime.Now.Date.AddDays(1);

            return filter;
        }

        // Set date selecting shorcut - last 12 months
        public static IFilter SetLast12Months(this IFilter filter)
        {
            filter.StartDateTime = DateTime.Now.Date.AddDays(-(int)DateTime.Now.Day + 1).AddMonths(-12);
            filter.EndDateTime = DateTime.Now.Date.AddDays(-(int)DateTime.Now.Day + 1);

            return filter;
        }

        // Set date selecting shorcut - last 24 months
        public static IFilter SetLast24Months(this IFilter filter)
        {
            filter.StartDateTime = DateTime.Now.Date.AddDays(-(int)DateTime.Now.Day + 1).AddMonths(-24);
            filter.EndDateTime = DateTime.Now.Date.AddDays(-(int)DateTime.Now.Day + 1);

            return filter;
        }
    }
}