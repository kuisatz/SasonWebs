using Antibiotic.Extensions;
using SasonWebs.Models.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace SasonWebs.Controllers.Performans.Models.Crm.Merkez
{
    public class Man_Projeler
    {
        public object Key { get; set; }
        public string Text { get; set; }

        public DateTime TARIH { get; set; }
        public decimal OWNERGROUP { get; set; }
        public decimal KAZ_PRJ_AD { get; set; }
        public decimal KAZ_PRJ_ARAC_AD { get; set; }
        public decimal TAM_AKT { get; set; }
        public decimal OLUS_PRJ { get; set; }
        public decimal VAZG_PRJ { get; set; }
        public decimal KYB_PRJ { get; set; }

        static MethodReturn<List<Man_Projeler>> Select_ManProjeler(DateTimeBetween dateTimeBetween, List<decimal> bayiIdler)
        {
            MethodReturn<List<Man_Projeler>> mr = new MethodReturn<List<Man_Projeler>>(); //User
            if (bayiIdler.isEmpty())
                return mr;
            using (var ap = new AppPoolMask<SasonWebAppPool>())
            {
                if (ap.AppPool.isNull())
                    ap.AppPool = SasonWebAppPool.Create;

                string bayiIdQuery = "";
                if (bayiIdler.Count >= 1)
                {
                    bayiIdQuery = @" and OWNERGROUP IN (" + bayiIdler.joinNumeric(",") + @") ";
                    if (bayiIdler.Count == 1 && bayiIdler[0] == 0)
                        bayiIdQuery = "";
                }

                mr.Result = ap.AppPool.EbaTestConnector.CreateQuery(@"
                    select 
                        sat.*, tarih 
                    from (select tarih from tarihler where tarih between {startDate} and {finishDate}) tarihler
                        left join mobilsatistablo@crm.oracle sat on sat.creation_date = tarihler.tarih 
                    order by 
                        sat.creation_date
                    ")
                   .Parameter("startDate", dateTimeBetween.Start.startOfDay())
                   .Parameter("finishDate", dateTimeBetween.Finish.endOfDay())
                   .GetDataTable(mr)
                   .ToModels<Man_Projeler>();
            }
            return mr;
        }

        public static ApiMethodReturn<List<Man_Projeler_GridData>> GetManProjelerGridData(List<decimal> bayiIdler)
        {
            ApiMethodReturn<List<Man_Projeler_GridData>> result = new ApiMethodReturn<List<Man_Projeler_GridData>>();
            MethodReturn<List<Man_Projeler>> mr = Select_ManProjeler(new DateTimeBetween(DateTime.Now.startOfYear(), DateTime.Now.endOfDay()), bayiIdler);
            if (mr.ok())
            {
                List<Man_Projeler> source = mr.Result.createIsNull();
                if (source.isNotEmpty())
                {
                    //Man_SalesFunnel.Select_Sales_Funnel(new DateTimeBetween(DateTime.Now.startOfYear(), DateTime.Now.endOfDay()), bayiIdler)

                    List<string> keys = new List<string>().addRange("KAZ_PRJ_AD", "KAZ_PRJ_ARAC_AD", "TAM_AKT", "OLUS_PRJ", "VAZG_PRJ", "KYB_PRJ");
                    List<string> texts = new List<string>().addRange("Kazanılan Proje Sayısı", "Kazanılan Projelerdeki Araç Sayısı", "Tamamlanan Aktivite Sayısı", "Oluşturulan Proje Sayısı", "Vazgeçilen Proje Sayısı", "Kaybedilen Proje Sayısı");

                    List<DateTimeBetween> dates = new List<DateTimeBetween>()
                        .add(new DateTimeBetween(DateTime.Now.startOfDay(), DateTime.Now.endOfDay()))
                        .add(new DateTimeBetween(DateTime.Now.startOfWeek(), DateTime.Now.endOfDay()))
                        .add(new DateTimeBetween(DateTime.Now.startOfMonth(), DateTime.Now.endOfDay()))
                        .add(new DateTimeBetween(DateTime.Now.startOfYear(), DateTime.Now.endOfDay()));

                    List<string> targets = new List<string>().addRange("Bugun", "BuHafta", "BuAy", "BuYil");

                    int keyIndex = 0;
                    keys.forEach(key =>
                    {
                        Man_Projeler_GridData keyRow = new Man_Projeler_GridData() { Key = key, Text = texts.value(keyIndex) }; //create a row

                        int dtIndex = 0;
                        dates.forEach(dt =>
                        {
                            decimal dtSayi = source.where(t => t.TARIH.isWithInRange(dt.Start, dt.Finish)).Sum(t => t.GetPropertyValue(key).cto<decimal>());
                            keyRow.SetPropertyValue(targets[dtIndex], dtSayi);
                            dtIndex++;
                        });

                        result.Data.add(keyRow);
                        keyIndex++;
                    });


                    result.Data.forEach(d =>
                    {
                        switch (d.Key)
                        {
                            case "KAZ_PRJ_AD":
                                d.Hedef = "581";
                                break;
                            case "KAZ_PRJ_ARAC_AD":
                                d.Hedef = "1820";
                                break;
                            case "TAM_AKT":
                                d.Hedef = "26921";
                                break;
                            case "OLUS_PRJ":
                                d.Hedef = "2154";
                                break;
                        }
                    });

                }
            }
            else
            {
                result.Exception = mr.ExceptionString;
            }
            return result;
        }

        public static ApiMethodReturn<List<Man_Projeler_GraphData>> GetManProjelerGraphData(List<decimal> bayiIdler)
        {
            ApiMethodReturn<List<Man_Projeler_GraphData>> result = new ApiMethodReturn<List<Man_Projeler_GraphData>>();
            MethodReturn<List<Man_Projeler>> mr = Select_ManProjeler(new DateTimeBetween(DateTime.Now.startOfYear(), DateTime.Now.endOfDay()), bayiIdler);
            if (mr.ok())
            {
                List<Man_Projeler> source = mr.Result.createIsNull();
                if (source.isNotEmpty())
                {
                    List<string> keys = new List<string>().addRange("KAZ_PRJ_AD", "KAZ_PRJ_ARAC_AD", "TAM_AKT", "OLUS_PRJ", "VAZG_PRJ", "KYB_PRJ");
                    List<string> texts = new List<string>().addRange("Kazanılan Proje Sayısı", "Kazanılan Projelerdeki Araç Sayısı", "Tamamlanan Aktivite Sayısı", "Oluşturulan Proje Sayısı", "Vazgeçilen Proje Sayısı", "Kaybedilen Proje Sayısı");

                    List<DateTimeBetween> dates = new List<DateTimeBetween>()
                        .add(new DateTimeBetween(DateTime.Now.startOfYear(), DateTime.Now.endOfDay()));

                    int keyIndex = 0;
                    keys.forEach(key =>
                    {
                        Man_Projeler_GraphData keyRow = new Man_Projeler_GraphData() { Key = key, Text = texts.value(keyIndex) }; //create a row

                        int dtIndex = 0;
                        dates.forEach(dt =>
                        {
                            decimal dtSayi = source.where(t => t.TARIH.isWithInRange(dt.Start, dt.Finish)).Sum(t => t.GetPropertyValue(key).cto<decimal>());
                            keyRow.Total = dtSayi;

                            source.ConvertTo(eTimeGroup.Month).forEach(monthlyData =>
                            {
                                keyRow.Value.add(monthlyData.GetPropertyValue(key).cto<decimal>());
                            });

                            dtIndex++;
                        });

                        result.Data.add(keyRow);
                        keyIndex++;
                    });
                }
            }
            else
            {
                result.Exception = mr.ExceptionString;
            }
            return result;
        }
    }


    public class Man_Projeler_GraphData
    {
        public string Key { get; set; }
        public string Text { get; set; }
        public decimal Total { get; set; }
        public List<decimal> Value { get; set; } = new List<decimal>();
    }

    public class Man_Projeler_GridData
    {
        public string Key { get; set; }
        public string Text { get; set; }

        public decimal Bugun { get; set; }
        public decimal BuHafta { get; set; }
        public decimal BuAy { get; set; }
        public decimal BuYil { get; set; }
        public string Hedef { get; set; }
    }

    //public class CrmMobilSatisData
    //{
    //    public decimal MaxValue { get; set; }

    //    private List<CrmMobilSatis> _Data;
    //    public List<CrmMobilSatis> Data
    //    {
    //        get { return _Data; }
    //        set
    //        {
    //            _Data = value;
    //            //if (_Data.isNotEmpty())
    //            //    MaxValue = _Data.Max(t => t.ISEMIR_KALAN + t.ISEMIR_KAPANAN);
    //        }
    //    }
    //}


    public static class CrmMobilSatisHelper
    {
        //public static List<Man_Projeler_GraphData> ConvertToGraphData(this List<Man_Projeler> source)
        //{
        //    List<Man_Projeler_GraphData> ret = new List<Man_Projeler_GraphData>();
        //    if (source.isEmpty()) return ret;

        //    List<string> keys = new List<string>().addRange("KAZ_PRJ_AD", "KAZ_PRJ_ARAC_AD", "TAM_AKT", "OLUS_PRJ", "VAZG_PRJ", "KYB_PRJ");
        //    List<string> texts = new List<string>().addRange("Kazanılan Proje Sayısı", "Kazanılan Projelerdeki Araç Sayısı", "Tamamlanan Aktivite Sayısı", "Oluşturulan Proje Sayısı", "Vazgeçilen Proje Sayısı", "Kaybedilen Proje Sayısı");
        //    int keyIndex = 0;
        //    keys.forEach(key =>
        //    {
        //        Man_Projeler_GraphData keyData = new Man_Projeler_GraphData() { Key = key, Text = texts.value(keyIndex) };
        //        source.forEach(data =>
        //        {
        //            keyData.Value.Add(data.GetPropertyValue<decimal>(key));
        //        });
        //        if (keyData.Value.isNotEmpty())
        //            keyData.Total = keyData.Value.Sum();
        //        ret.add(keyData);
        //        keyIndex++;
        //    });
        //    return ret;
        //}

        public static List<Man_Projeler> ConvertTo(this List<Man_Projeler> source, eTimeGroup gruplama)
        {
            List<Man_Projeler> ret = new List<Man_Projeler>();
            if (source.isEmpty()) return ret;

            Man_Projeler row = null;
            switch (gruplama)
            {
                case eTimeGroup.Day:
                    source.GroupBy(t => t.TARIH).forEach(keyValue =>
                    {
                        row = new Man_Projeler() { Key = keyValue.Key.toLongD(), Text = keyValue.Key.ToString("dd.MM.yyyy") };
                        ret.add(row);
                        keyValue.forEach(value => row.Append(value));
                    });
                    break;
                case eTimeGroup.Week:
                    source.GroupBy(t => $"{t.TARIH.Year}-{t.TARIH.weekOfYear().ToString("00")}").forEach(keyValue =>
                    {
                        row = new Man_Projeler() { Key = keyValue.Key, Text = keyValue.Key };
                        ret.add(row);
                        keyValue.forEach(value => row.Append(value));
                    });
                    break;
                case eTimeGroup.Month:
                    source.GroupBy(t => $"{t.TARIH.Year}-{t.TARIH.Month.ToString("00")}").forEach(keyValue =>
                    {
                        row = new Man_Projeler() { Key = keyValue.Key, Text = keyValue.Key };
                        ret.add(row);
                        keyValue.forEach(value => row.Append(value));
                    });
                    break;
                case eTimeGroup.Year:
                    source.GroupBy(t => t.TARIH.Year).forEach(keyValue =>
                    {
                        row = new Man_Projeler() { Key = keyValue.Key, Text = keyValue.Key.toString() };
                        ret.add(row);
                        keyValue.forEach(value => row.Append(value));
                    });
                    break;
                case eTimeGroup.Total:
                    row = new Man_Projeler() { Key = "TOTAL", Text = "TOTAL" };
                    source.forEach(value => row.Append(value));
                    ret.add(row);
                    break;
            }
            return ret;
        }


        public static Man_Projeler Append(this Man_Projeler target, Man_Projeler mobilSatis)
        {
            if (target != null)
            {
                target.KAZ_PRJ_AD += mobilSatis.KAZ_PRJ_AD;
                target.KAZ_PRJ_ARAC_AD += mobilSatis.KAZ_PRJ_ARAC_AD;
                target.TAM_AKT += mobilSatis.TAM_AKT;
                target.OLUS_PRJ += mobilSatis.OLUS_PRJ;
                target.VAZG_PRJ += mobilSatis.VAZG_PRJ;
                target.KYB_PRJ += mobilSatis.KYB_PRJ;
            }
            return target;
        }

    }
}