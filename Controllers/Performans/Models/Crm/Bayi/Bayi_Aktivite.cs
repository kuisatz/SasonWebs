using Antibiotic.Extensions;
using SasonWebs.Models.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SasonWebs.Controllers.Performans.Models.Crm.Bayi
{
    public class Bayi_Aktivite
    {
        public object Key { get; set; }
        public string Text { get; set; }

        public decimal ADET { get; set; }
        public decimal ACTIVITY_TYPE { get; set; }
        public decimal OWNERGROUP { get; set; }
        public DateTime ACTIVITY_DATE { get; set; }

        static MethodReturn<List<Bayi_Aktivite>> Select_Bayiler_Actities(List<decimal> activityTypes, DateTimeBetween dateTimeBetween, List<decimal> bayiIdler)
        {
            MethodReturn<List<Bayi_Aktivite>> mr = new MethodReturn<List<Bayi_Aktivite>>();
            if (bayiIdler.isNotEmpty() && bayiIdler.Count == 1 && bayiIdler[0] == 0) //Admin
            {
                string activityTypesQuery = "";
                if (activityTypes.isNotEmpty())
                    activityTypesQuery = " and status in (" + activityTypes.joinNumeric(",") + ") ";

                using (var ap = SasonWebAppPool.CreateMask)
                {
                    mr.Result = ap.AppPool.EbaTestConnector.CreateQuery(@"
                         select count(trunc(activity_r_date)) adet,activity_type,ownergroup,trunc(activity_r_date) ACTIVITY_DATE
                         from activity@crm.oracle
                         where  
                            activity_r_date between {startDate} and {finishDate} 
                         and product=1 
                         "+activityTypesQuery+@"
                         group by activity_type,ownergroup,trunc(activity_r_date),trunc(activity_r_date)
                         order by trunc(activity_r_date)
                    ")
                    .Parameter("startDate", dateTimeBetween.Start.startOfDay())
                    .Parameter("finishDate", dateTimeBetween.Finish.endOfDay())
                    .GetDataTable(mr)
                    .ToModels<Bayi_Aktivite>();
                }
            }
            return mr;
        }

        public static ApiMethodReturn<List<Card>> GetMainCards(List<decimal> bayiIdler)
        {
            ApiMethodReturn<List<Card>> ret = new ApiMethodReturn<List<Card>>();
            MethodReturn<List<Bayi_Aktivite>> mr = Select_Bayiler_Actities(new List<decimal>().addRange(1, 2, 3, 4), new DateTimeBetween(DateTime.Now.startOfDay(), DateTime.Now.endOfDay()), bayiIdler);
            if (mr.ok())
            {
                decimal total = 0;
                List<Bayi_Aktivite> dataSource = mr.Result.createIsNull().ConvertTo(eTimeGroup.Total);
                if (dataSource.isNotEmpty())
                    total = dataSource.Sum(t => t.ADET);
                ret.Data.add(new Card() { Id = "BAYI_AKTIVITELER", Text = "Bayi Aktiviteler", Value = total.cto<int>().ToString(Statics.Nf), Type = "ADET" });
                ret.Data.forEach(t =>
                {
                    //t.HasDetails = true;
                    //t.DefaultTimeType = Enum.GetName(typeof(eTimes), eTimes.Last_7_Days);
                    //t.Possible = EnumsHelper.EnumZamanTipiAdlari(eTimes.Last_7_Days, eTimes.Last_1_Month, eTimes.Last_6_Months, eTimes.Last_1_Year);
                });
            }
            else
            {
                ret.Exception = mr.ExceptionString;
            }
            return ret;
        }


        public static ApiMethodReturn<List<Bayi_Aktivite_GraphData>> GetCardDetails(eTimes timeType, List<decimal> bayiIdler)
        {
            MethodReturn<List<Bayi_Aktivite>> mr = Select_Bayiler_Actities(new List<decimal>().addRange(1, 2, 3,4), timeType.ToDateTimeBetween(), bayiIdler);
            ApiMethodReturn<List<Bayi_Aktivite_GraphData>> result = new ApiMethodReturn<List<Bayi_Aktivite_GraphData>>();
            if (mr.ok())
            {
                List<Bayi_Aktivite> convertedData = mr.Result.ConvertTo(timeType.ToGroup());
                //result.Data = new FirsatData() { Data = mr.Result.createIsNull().ConvertTo(timeType.ToGroup()) };
                convertedData.forEach(cdata =>
                {
                    result.Data.add(new Bayi_Aktivite_GraphData()
                    {
                        Key = cdata.Key.toString(),
                        Text = cdata.Text.toString(),
                        Value = cdata.ADET,
                    });
                });
            }
            else
            {
                result.Exception = mr.ExceptionString;
            }
            return result;
        }
    }


    public class Bayi_Aktivite_GraphData
    {
        public string Key { get; set; }
        public string Text { get; set; }
        public decimal Value { get; set; }
    }




    public static class Bayi_Aktivite_Helper
    {
        public static List<Bayi_Aktivite> ConvertTo(this List<Bayi_Aktivite> source, eTimeGroup gruplama)
        {
            List<Bayi_Aktivite> ret = new List<Bayi_Aktivite>();
            if (source.isEmpty()) return ret;

            Bayi_Aktivite row = null;
            switch (gruplama)
            {
                case eTimeGroup.Day:
                    source.GroupBy(t => t.ACTIVITY_DATE.Date).forEach(keyValue =>
                    {
                        row = new Bayi_Aktivite() { Key = keyValue.Key.toLongD(), Text = keyValue.Key.ToString("dd.MM.yyyy") };
                        ret.add(row);
                        keyValue.forEach(value => row.Append(value));
                    });
                    break;
                case eTimeGroup.Week:
                    source.GroupBy(t => $"{t.ACTIVITY_DATE.Year}-{t.ACTIVITY_DATE.weekOfYear().ToString("00")}").forEach(keyValue =>
                    {
                        row = new Bayi_Aktivite() { Key = keyValue.Key, Text = keyValue.Key };
                        ret.add(row);
                        keyValue.forEach(value => row.Append(value));
                    });
                    break;
                case eTimeGroup.Month:
                    source.GroupBy(t => $"{t.ACTIVITY_DATE.Year}-{t.ACTIVITY_DATE.Month.ToString("00")}").forEach(keyValue =>
                    {
                        row = new Bayi_Aktivite() { Key = keyValue.Key, Text = keyValue.Key };
                        ret.add(row);
                        keyValue.forEach(value => row.Append(value));
                    });
                    break;
                case eTimeGroup.Year:
                    source.GroupBy(t => t.ACTIVITY_DATE.Year).forEach(keyValue =>
                    {
                        row = new Bayi_Aktivite() { Key = keyValue.Key, Text = keyValue.Key.toString() };
                        ret.add(row);
                        keyValue.forEach(value => row.Append(value));
                    });
                    break;
                case eTimeGroup.Total:
                    row = new Bayi_Aktivite() { Key = "TOTAL", Text = "TOTAL" };
                    source.forEach(value => row.Append(value));
                    ret.add(row);
                    break;
            }
            return ret;
        }

        public static Bayi_Aktivite Append(this Bayi_Aktivite target, Bayi_Aktivite item)
        {
            if (target.isNotNull())
            {
                target.ADET += item.ADET;
            }
            return target;
        }
    }


}
