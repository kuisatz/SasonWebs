using Antibiotic.Extensions;
using SasonWebs.Models.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SasonWebs.Controllers.Performans.Models.Crm.Bayi
{
    /// <summary>
    /// status = 
    ///     select * from lov_opportunity_status@crm.oracle where lang = 'tr';
    /// </summary>

    public class Bayi_Firsatlar
    {
        public object Key { get; set; }
        public string Text { get; set; }

        public decimal  PRJ_SAYISI { get; set; }
        public decimal  STATUS { get; set; }
        public DateTime CREATION_DATE { get; set; }
        public decimal  OWNERGROUP { get; set; }

        static MethodReturn<List<Bayi_Firsatlar>> Select_Firsatlar(List<decimal> status, DateTimeBetween dateTimeBetween, List<decimal> bayiIdler)
        {
            MethodReturn<List<Bayi_Firsatlar>> mr = new MethodReturn<List<Bayi_Firsatlar>>();
            if (bayiIdler.isNotEmpty() && bayiIdler.Count == 1 && bayiIdler[0] == 0) //Admin
            {
                string statusQuery = "";
                if (status.isNotEmpty())
                    statusQuery = " and status in ("+status.joinNumeric(",")+") ";

                using (var ap = SasonWebAppPool.CreateMask)
                {
                    mr.Result = ap.AppPool.EbaTestConnector.CreateQuery(@"
                        select count(opportunity_id) prj_sayisi, status,creation_date,ownergroup
                         from opportunity@crm.oracle
                         where 
                         creation_date between {startDate} and {finishDate}
                        "+statusQuery+@" 
                        and product=1
                        group by creation_date,ownergroup,status
                        order by creation_date
                    ")
                    .Parameter("startDate", dateTimeBetween.Start.startOfDay())
                    .Parameter("finishDate", dateTimeBetween.Finish.endOfDay())
                    .GetDataTable(mr)
                    .ToModels<Bayi_Firsatlar>();
                }
            }
            return mr;
        }

        public static ApiMethodReturn<List<Card>> GetMainCards(List<decimal> bayiIdler)
        {
            ApiMethodReturn<List<Card>> ret = new ApiMethodReturn<List<Card>>();
            MethodReturn<List<Bayi_Firsatlar>> mr = Select_Firsatlar(new List<decimal>().addRange(1,2,3), eTimes.Last_7_Days.ToDateTimeBetween(), bayiIdler);
            if (mr.ok())
            {
                decimal total = 0;
                List<Bayi_Firsatlar> dataSource = mr.Result.createIsNull().ConvertTo(eTimeGroup.Total);
                if (dataSource.isNotEmpty())
                    total = dataSource.Sum(t => t.PRJ_SAYISI);
                ret.Data.add(new Card() { Id = "BAYI_FIRSATLAR", Text = "Fırsatlar", Value = total.ToString(Statics.Nf), Type = "ADET" });
                ret.Data.forEach(t =>
                {
                    //t.HasDetails      = true;
                    //t.DefaultTimeType = Enum.GetName(typeof(eTimes), eTimes.Last_7_Days);
                    //t.Possible        = EnumsHelper.EnumZamanTipiAdlari(eTimes.Last_7_Days, eTimes.Last_1_Month, eTimes.Last_6_Months, eTimes.Last_1_Year);
                });
            }
            else
            {
                ret.Exception = mr.ExceptionString;
            }
            return ret;
        }

        public static ApiMethodReturn<List<Bayi_Firsatlar_GraphData>> GetCardDetails(eTimes timeType, List<decimal> bayiIdler)
        {
            MethodReturn<List<Bayi_Firsatlar>> mr = Select_Firsatlar(new List<decimal>().addRange(1,2,3), timeType.ToDateTimeBetween(), bayiIdler);
            ApiMethodReturn<List<Bayi_Firsatlar_GraphData>> result = new ApiMethodReturn<List<Bayi_Firsatlar_GraphData>>();
            if (mr.ok())
            {
                List<Bayi_Firsatlar> convertedData = mr.Result.ConvertTo(timeType.ToGroup());
                //result.Data = new FirsatData() { Data = mr.Result.createIsNull().ConvertTo(timeType.ToGroup()) };
                convertedData.forEach(cdata =>
                {
                    result.Data.add(new Bayi_Firsatlar_GraphData()
                    {
                        Key = cdata.Key.toString(),
                        Text = cdata.Text.toString(),
                        Value = cdata.PRJ_SAYISI,
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


    public class Bayi_Firsatlar_GraphData
    {
        public string Key { get; set; }
        public string Text { get; set; }
        public decimal Value { get; set; }
    }


    public static class Bayi_Firsatlar_Helper
    {
        public static List<Bayi_Firsatlar> ConvertTo(this List<Bayi_Firsatlar> source, eTimeGroup gruplama)
        {
            List<Bayi_Firsatlar> ret = new List<Bayi_Firsatlar>();
            if (source.isEmpty()) return ret;

            Bayi_Firsatlar row = null;
            switch (gruplama)
            {
                case eTimeGroup.Day:
                    source.GroupBy(t => t.CREATION_DATE.Date).forEach(keyValue =>
                    {
                        row = new Bayi_Firsatlar() { Key = keyValue.Key.toLongD(), Text = keyValue.Key.ToString("dd.MM.yyyy") };
                        ret.add(row);
                        keyValue.forEach(value => row.Append(value));
                    });
                    break;
                case eTimeGroup.Week:
                    source.GroupBy(t => $"{t.CREATION_DATE.Year}-{t.CREATION_DATE.weekOfYear().ToString("00")}").forEach(keyValue =>
                    {
                        row = new Bayi_Firsatlar() { Key = keyValue.Key, Text = keyValue.Key };
                        ret.add(row);
                        keyValue.forEach(value => row.Append(value));
                    });
                    break;
                case eTimeGroup.Month:
                    source.GroupBy(t => $"{t.CREATION_DATE.Year}-{t.CREATION_DATE.Month.ToString("00")}").forEach(keyValue =>
                    {
                        row = new Bayi_Firsatlar() { Key = keyValue.Key, Text = keyValue.Key };
                        ret.add(row);
                        keyValue.forEach(value => row.Append(value));
                    });
                    break;
                case eTimeGroup.Year:
                    source.GroupBy(t => t.CREATION_DATE.Year).forEach(keyValue =>
                    {
                        row = new Bayi_Firsatlar() { Key = keyValue.Key, Text = keyValue.Key.toString() };
                        ret.add(row);
                        keyValue.forEach(value => row.Append(value));
                    });
                    break;
                case eTimeGroup.Total:
                    row = new Bayi_Firsatlar() { Key = "TOTAL", Text = "TOTAL" };
                    source.forEach(value => row.Append(value));
                    ret.add(row);
                    break;
            }
            return ret;
        }

        public static Bayi_Firsatlar Append(this Bayi_Firsatlar target, Bayi_Firsatlar item)
        {
            if (target.isNotNull())
            {
                target.PRJ_SAYISI += item.PRJ_SAYISI;
            }
            return target;
        }
    }
}