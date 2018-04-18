using Antibiotic.Extensions;
using SasonWebs.Models.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SasonWebs.Controllers.Performans.Models.Sason
{
    public class Sason_Musteriler
    {
        public decimal SERVISID { get; set; }
        public string AD { get; set; }
        public string MUSTERIAD { get; set; }
        public string MUSTERITELEFON { get; set; }
        public string SASENO { get; set; }
        public string PLAKA { get; set; }

        public static MethodReturn<List<Sason_Musteriler>> Select_Musteriler(DateTimeBetween dateTimeBetween, List<decimal> servisIds)
        {
            MethodReturn<List<Sason_Musteriler>> mr = new MethodReturn<List<Sason_Musteriler>>(); //User
            using (var ap = new AppPoolMask<SasonWebAppPool>())
            {
                if (ap.AppPool.isNull())
                    ap.AppPool = SasonWebAppPool.Create;
                mr.Result = ap.AppPool.EbaTestConnector.CreateQuery(@"
                    SELECT distinct s.servisid,
                              o.ad,
                               S.MUSTERIAD,
                              S.MUSTERITELEFON,
                              s.saseno,R.PLAKA
                              from sason.vt_servisler t,sason.servisisemirler s,sason.vt_servisisortaklar o ,sason.servisvarlikruhsatlar r,sason.vw_isortakkontakbilgiler h
                              where O.SERVISISORTAKID =S.SERVISISORTAKID and s.servisid=t.servisid
                              and t.dilkod='Turkish' and S.SASENO=R.SASENO
                              and trunc(s.kayittarih) between {startDate} and {finishDate}
                              and t.dilkod=o.dilkod 
                             and  ( s.servisid in (" + servisIds.joinNumeric(",")  +@"))
                              and O.SERVISISORTAKID=h.servisisortakid(+)
                             and (h.servisisortakkontaktipid=6 or h.servisisortakkontaktipid is null)                    
                    ")
                   .Parameter("startDate", dateTimeBetween.Start.startOfDay())
                   .Parameter("finishDate", dateTimeBetween.Finish.endOfDay())
                   .GetDataTable(mr)
                   .ToModels<Sason_Musteriler>();
            }
            return mr;
        }


        public static ApiMethodReturn<List<Card>> GetMainCards(DateTime startDate, DateTime finishDate, List<decimal> servisIds)
        {
            ApiMethodReturn<List<Card>> ret = new ApiMethodReturn<List<Card>>();
            MethodReturn<List<Sason_Musteriler>> mr = Select_Musteriler(new DateTimeBetween(startDate, finishDate), servisIds);
            if (mr.ok())
            {
                ret.Data.add(new Card() { Id = "MUSTERI_SAYISI", Text = "Müşteri Sayısı", Value = mr.Result?.Count.ToString(Statics.Nf), Type = "ADET" });
                ret.Data.forEach(t =>
                {
                    //t.HasDetails = true;
                    //t.DefaultTimeType = Enum.GetName(typeof(eTimes), eTimes.Last_7_Days);
                    //t.Possible = EnumsHelper.EnumZamanTipiAdlari(eTimes.Today, eTimes.ThisWeek, eTimes.ThisMonth, eTimes.ThisYear);
                });
            }
            else
            {
                ret.Exception = mr.ExceptionString;
            }
            return ret;
        }
    }


    //public static class MusterilerHelper
    //{
    //    public static List<Musteriler> ConvertTo(this List<Musteriler> source, eTimeGroup gruplama)
    //    {
    //        List<Musteriler> ret = new List<Musteriler>();
    //        if (source.isEmpty()) return ret;

    //        Musteriler row = null;
    //        switch (gruplama)
    //        {
    //            case eTimeGroup.Day:
    //                source.GroupBy(t => t.TARIH).forEach(keyValue =>
    //                {
    //                    row = new Musteriler() { Key = keyValue.Key.toLongD(), Text = keyValue.Key.ToString("dd.MM.yyyy") };
    //                    ret.add(row);
    //                    keyValue.forEach(value => row.Append(value));
    //                });
    //                break;
    //            case eTimeGroup.Week:
    //                source.GroupBy(t => $"{t.TARIH.Year}.{t.TARIH.weekOfYear().ToString("00")}").forEach(keyValue =>
    //                {
    //                    row = new Musteriler() { Key = keyValue.Key, Text = keyValue.Key };
    //                    ret.add(row);
    //                    keyValue.forEach(value => row.Append(value));
    //                });
    //                break;
    //            case eTimeGroup.Month:
    //                source.GroupBy(t => $"{t.TARIH.Year}.{t.TARIH.Month.ToString("00")}").forEach(keyValue =>
    //                {
    //                    row = new Musteriler() { Key = keyValue.Key, Text = keyValue.Key };
    //                    ret.add(row);
    //                    keyValue.forEach(value => row.Append(value));
    //                });
    //                break;
    //            case eTimeGroup.Year:
    //                source.GroupBy(t => t.TARIH.Year).forEach(keyValue =>
    //                {
    //                    row = new Musteriler() { Key = keyValue.Key, Text = keyValue.Key.toString() };
    //                    ret.add(row);
    //                    keyValue.forEach(value => row.Append(value));
    //                });
    //                break;
    //            case eTimeGroup.Total:
    //                row = new Musteriler() { Key = "TOTAL", Text = "TOTAL" };
    //                source.forEach(value => row.Append(value));
    //                ret.add(row);
    //                break;
    //        }
    //        return ret;
    //    }

    //    public static Musteriler Append(this Musteriler target, Musteriler mobilAGS)
    //    {
    //        if (target.isNotNull())
    //        {
    //            target.ARAC_GIRIS += mobilAGS.ARAC_GIRIS;
    //            target.ISEMIR_ACILAN += mobilAGS.ISEMIR_ACILAN;
    //            target.ISEMIR_KAPANAN += mobilAGS.ISEMIR_KAPANAN;
    //        }
    //        return target;
    //    }
    //}
}
