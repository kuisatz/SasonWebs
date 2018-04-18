using Antibiotic.Extensions;
using SasonWebs.Models.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SasonWebs.Controllers.Performans.Models.Crm.Merkez
{
    public class Man_Ureticiler_Satis
    {
        public object Key { get; set; }
        public string Text { get; set; }

        public string MARKA { get; set; }
        public decimal YILAY { get; set; }
        public decimal ADET { get; set; }

        public DateTime TARIH
        {
            get
            {
                bool isdate = false;
                return $"{YILAY}01".toDateTime("yyyyMMdd", out isdate); 
            }
        }

        static MethodReturn<List<Man_Ureticiler_Satis>> Select_Uretici_SatisSayilari(List<string> markalar, DateTimeBetween dateTimeBetween, List<decimal> bayiIdler)
        {
            MethodReturn<List<Man_Ureticiler_Satis>> mr = new MethodReturn<List<Man_Ureticiler_Satis>>();
            if (bayiIdler.isNotEmpty() && bayiIdler.Count == 1 && bayiIdler[0] == 0) //Admin
            {
                string markalarQuery = "";
                if (markalar.isNotEmpty())
                    markalarQuery = " and MARKA in (" + markalar.join(",") + ") ";

                using (var ap = SasonWebAppPool.CreateMask)
                {
                    mr.Result = ap.AppPool.EbaTestConnector.CreateQuery(@"
                        select * from mobilpazar where 1=1 "+markalarQuery+@" order by MARKA, YILAY
                    ")
                    .Parameter("startDate", dateTimeBetween.Start.startOfDay())
                    .Parameter("finishDate", dateTimeBetween.Finish.endOfDay())
                    .GetDataTable(mr)
                    .ToModels<Man_Ureticiler_Satis>();
                }
            }
            return mr;
        }

        public static ApiMethodReturn<List<Ureticiler_Satis_GraphData>> GetUreticilerGraphData(List<decimal> bayiIdler)
        {
            MethodReturn<List<Man_Ureticiler_Satis>> mr = Select_Uretici_SatisSayilari(null, new DateTimeBetween(), bayiIdler);
            ApiMethodReturn<List<Ureticiler_Satis_GraphData>> result = new ApiMethodReturn<List<Ureticiler_Satis_GraphData>>();
            if (mr.ok())
            {
                List<Man_Ureticiler_Satis> data = mr.Result.createIsNull();

                data.GroupBy(gYear => gYear.TARIH.Year).forEach(gYearDet =>
                {
                    Ureticiler_Satis_GraphData yilData = new Ureticiler_Satis_GraphData() { Key = gYearDet.Key.ToString() };
                    gYearDet.GroupBy(gUretici => gUretici.MARKA).forEach(gUreticiMarkaDet =>
                    {
                        Ureticiler_Satis_GraphRow m2 = new Ureticiler_Satis_GraphRow() { Text = gUreticiMarkaDet.Key };
                        gUreticiMarkaDet.forEach(t =>
                        {
                            m2.Value += t.ADET;
                        });
                        yilData.Value.add(m2);
                    });
                    result.Data.add(yilData);
                });
            }
            else
            {
                result.Exception = mr.ExceptionString;
            }
            return result;
        }

        public static ApiMethodReturn<List<Ureticiler_Satis_GraphData>> GetUreticilerBarGraphData(List<decimal> bayiIdler)
        {
            MethodReturn<List<Man_Ureticiler_Satis>> mr = Select_Uretici_SatisSayilari(null, new DateTimeBetween(), bayiIdler);
            ApiMethodReturn<List<Ureticiler_Satis_GraphData>> result = new ApiMethodReturn<List<Ureticiler_Satis_GraphData>>();
            if (mr.ok())
            {
                List<Man_Ureticiler_Satis> data = mr.Result.createIsNull();

                data.GroupBy(gYear => gYear.MARKA).forEach(gYearDet =>
                {
                    Ureticiler_Satis_GraphData yilData = new Ureticiler_Satis_GraphData() { Key = gYearDet.Key.ToString() };
                    gYearDet.GroupBy(gUretici => gUretici.TARIH.Year).forEach(gUreticiMarkaDet =>
                    {
                        Ureticiler_Satis_GraphRow m2 = new Ureticiler_Satis_GraphRow() { Text = gUreticiMarkaDet.Key.ToString() };
                        gUreticiMarkaDet.forEach(t =>
                        {
                            m2.Value += t.ADET;
                        });
                        yilData.Value.add(m2);
                    });
                    result.Data.add(yilData);
                });
            }
            else
            {
                result.Exception = mr.ExceptionString;
            }
            return result;
        }

        public static ApiMethodReturn<List<Ureticiler_Satis_GraphData>> GetManSatisSayiBar(List<decimal> bayiIdler)
        {
            MethodReturn<List<Man_Ureticiler_Satis>> mr = Select_Uretici_SatisSayilari(new List<string>().add("MAN"), new DateTimeBetween(), bayiIdler);
            ApiMethodReturn<List<Ureticiler_Satis_GraphData>> result = new ApiMethodReturn<List<Ureticiler_Satis_GraphData>>();
            if (mr.ok())
            {
                List<Man_Ureticiler_Satis> data = mr.Result.createIsNull();

                data.GroupBy(gYear => gYear.TARIH.Month).forEach(gYearDet =>
                {
                    Ureticiler_Satis_GraphData yilData = new Ureticiler_Satis_GraphData() { Key = gYearDet.Key.ToString() };
                    gYearDet.GroupBy(gUretici => gUretici.TARIH.Year).forEach(gUreticiMarkaDet =>
                    {
                        Ureticiler_Satis_GraphRow m2 = new Ureticiler_Satis_GraphRow() { Text = gUreticiMarkaDet.Key.ToString() };
                        gUreticiMarkaDet.forEach(t =>
                        {
                            m2.Value += t.ADET;
                        });
                        yilData.Value.add(m2);
                    });
                    result.Data.add(yilData);
                });
            }
            else
            {
                result.Exception = mr.ExceptionString;
            }
            return result;
        }
    }


    public class Ureticiler_Satis_GraphData
    {
        public string Key { get; set; }
        public List<Ureticiler_Satis_GraphRow> Value { get; set; } = new List<Ureticiler_Satis_GraphRow>();
    }

    public class Ureticiler_Satis_GraphRow
    {
        public string Text { get; set; }
        public decimal Value { get; set; }
    }



    //public static class Bayi_Firsatlar_Helper
    //{
    //    public static List<Ureticiler_Satis> ConvertTo(this List<Ureticiler_Satis> source, eTimeGroup gruplama)
    //    {
    //        List<Ureticiler_Satis> ret = new List<Ureticiler_Satis>();
    //        if (source.isEmpty()) return ret;

    //        Ureticiler_Satis row = null;
    //        switch (gruplama)
    //        {
    //            case eTimeGroup.Month:
    //                source.GroupBy(t => $"{t.TARIH.Year}-{t.TARIH.Month.ToString("00")}").forEach(keyValue =>
    //                {
    //                    row = new Ureticiler_Satis() { Key = keyValue.Key, Text = keyValue.Key };
    //                    ret.add(row);
    //                    keyValue.forEach(value => row.Append(value));
    //                });
    //                break;
    //            case eTimeGroup.Year:
    //                source.GroupBy(t => t.TARIH.Year).forEach(keyValue =>
    //                {
    //                    row = new Ureticiler_Satis() { Key = keyValue.Key, Text = keyValue.Key.toString() };
    //                    ret.add(row);
    //                    keyValue.forEach(value => row.Append(value));
    //                });
    //                break;
    //        }
    //        return ret;
    //    }

    //    public static Ureticiler_Satis Append(this Ureticiler_Satis target, Ureticiler_Satis item)
    //    {
    //        if (target.isNotNull())
    //        {
    //            target.ADET += item.ADET;
    //        }
    //        return target;
    //    }
    //}

}