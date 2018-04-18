using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Antibiotic.Helpers;
using Antibiotic.Extensions;
using SasonWebs.Controllers.Performans.Models;

namespace SasonWebs.Controllers.Performans
{
    public static class PerformansHelpers
    {
        static string parameterDateFormat = "yyyyMMdd";
        static string parameterDateTimeFormat = "yyyyMMddHHmm";

        public static DateTime ToParamDate(this string parameterDate)
        {
            bool isdate = false;
            return parameterDate.toDateTime(parameterDateFormat, out isdate);
        }

        public static DateTime ToParamDateTime(this string parameterDateTime)
        {
            bool isdate = false;
            return parameterDateTime.toDateTime(parameterDateTimeFormat, out isdate);
        }

        public static ServerUserInfo ToUser(this string userToken)
        {
            return Statics.GetServerUserInfo(userToken);
        }

        public static List<decimal> ToServisIds(this string servisIds, List<decimal> defaultServisIds)
        {
            List<decimal> retIds = servisIds.trim().split().select(t => t.cto<decimal>()).toList();
            if (retIds.isEmpty())
                return defaultServisIds;
            else
            {
                List<decimal> removeList = new List<decimal>();
                retIds.forEach(s =>
                {
                    if (defaultServisIds.notcontains(s))
                        removeList.add(s);
                });
                if (removeList.isNotEmpty())
                {
                    retIds.removes(removeList);
                    if (retIds.isEmpty())
                        retIds.add(0);
                }
            }
            return retIds;
        }

        public static T ToEnum<T>(this string source)
        {
            return (T)Enum.Parse(typeof(eTimes), source);
        }




        public static DateTimeBetween ToDateTimeBetween(this eTimes times)
        {
            DateTimeBetween ret = new DateTimeBetween();
            switch (times)
            {
                case eTimes.Now:
                    ret = new DateTimeBetween(DateTime.Now, DateTime.Now);
                    break;
                case eTimes.Last_7_Days:
                    ret = new DateTimeBetween(DateTime.Now.AddDays(-6).startOfDay(), DateTime.Now.endOfDay());
                    break;
                //case eTimes.Last_10_Days:
                //    ret = new DateTimeBetween(DateTime.Now.AddDays(-9).startOfDay(), DateTime.Now.endOfDay());
                //    break;
                case eTimes.Last_1_Month:
                    //ret = new DateTimeBetween(DateTime.Now.AddMonths(-1).startOfDay(), DateTime.Now.endOfDay());
                    ret = new DateTimeBetween(DateTime.Now.beforeWeek(3), DateTime.Now.endOfDay());
                    break;
                //case eTimes.Last_3_Months:
                //    ret = new DateTimeBetween(DateTime.Now.AddMonths(-2).startOfMonth().startOfDay(), DateTime.Now.endOfMonth());
                //    break;
                case eTimes.Last_6_Months:
                    ret = new DateTimeBetween(DateTime.Now.AddMonths(-5).startOfMonth().startOfDay(), DateTime.Now.endOfMonth());
                    break;
                case eTimes.Last_1_Year:
                    ret = new DateTimeBetween(DateTime.Now.AddMonths(-11).startOfDay(), DateTime.Now.endOfDay());
                    break;
                    //case eTimes.Today:
                    //    ret =new DateTimeBetween(DateTime.Now.startOfDay(), DateTime.Now.endOfDay());
                    //    break;
                    //case eTimes.ThisWeek:
                    //    ret = new DateTimeBetween(DateTime.Now.startOfWeek().startOfDay(), DateTime.Now.endOfDay());
                    //    break;
                    //case eTimes.ThisMonth:
                    //    ret = new DateTimeBetween(DateTime.Now.startOfMonth().startOfDay(), DateTime.Now.endOfDay());
                    //    break;
                    //case eTimes.ThisYear:
                    //    ret = new DateTimeBetween(DateTime.Now.startOfYear().startOfDay(), DateTime.Now.endOfDay());
                    //    break;
                    //case eTimes.Yesterday:
                    //    ret = new DateTimeBetween(DateTime.Now.beforeDate().startOfDay(), DateTime.Now.beforeDate().endOfDay());
                    //    break;
                    //case eTimes.LastWeek:
                    //    ret = new DateTimeBetween(DateTime.Now.lastWeek().startOfWeek().startOfDay(), DateTime.Now.lastWeek().endOfWeek().endOfDay());
                    //    break;
                    //case eTimes.LastMonth:
                    //    ret = new DateTimeBetween(DateTime.Now.beforeMonth().startOfMonth().startOfDay(), DateTime.Now.beforeMonth().endOfMonth().endOfDay());
                    //    break;
                    //case eTimes.LastYear:
                    //    ret = new DateTimeBetween(DateTime.Now.AddYears(-1).startOfYear().startOfDay(), DateTime.Now.AddYears(1).endOfYear().endOfDay());
                    //    break;
            }
            return ret;
        }

        public static eTimeGroup ToGroup(this eTimes eTimes)
        {
            eTimeGroup ret = eTimeGroup.Day;
            switch (eTimes)
            {
                //case eTimes.Today:
                //    ret = eTimeGroup.Day;
                //    break;
                //case eTimes.ThisWeek:
                //    ret = eTimeGroup.Day;
                //    break;
                //case eTimes.ThisMonth:
                //    ret = eTimeGroup.Day;
                //    break;
                //case eTimes.ThisYear:
                //    ret = eTimeGroup.Month;
                //    break;
                //case eTimes.Yesterday:
                //    ret = eTimeGroup.Day;
                //    break;
                //case eTimes.LastWeek:
                //    ret = eTimeGroup.Day;
                //    break;
                //case eTimes.LastMonth:
                //    ret = eTimeGroup.Day;
                //    break;
                //case eTimes.LastYear:
                //    ret = eTimeGroup.Month;
                //    break;
                //default:
                //    break;
                case eTimes.Now:
                    ret = eTimeGroup.Day;
                    break;
                case eTimes.Last_7_Days:
                //case eTimes.Last_10_Days:
                    ret = eTimeGroup.Day;
                    break;
                case eTimes.Last_1_Month:
                    ret = eTimeGroup.Week;
                    break;
                //case eTimes.Last_3_Months:
                case eTimes.Last_6_Months:
                case eTimes.Last_1_Year:
                    ret = eTimeGroup.Month;
                    break;
            }
            return ret;
        }

    }
}