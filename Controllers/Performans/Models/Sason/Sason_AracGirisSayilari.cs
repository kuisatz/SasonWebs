using Antibiotic.Extensions;
using SasonWebs.Models.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SasonWebs.Controllers.Performans.Models.Sason
{
    /// <summary>
    /// Test Edildi
    /// </summary>
    public class Sason_AracGirisSayilari
    {
        public object   Key { get; set; }
        public string   Text { get; set; }

        public DateTime TARIH { get; set; }
        public decimal  SERVISID { get; set; }
        public decimal  ARAC_GIRIS { get; set; }
        public decimal  ISEMIR_ACILAN { get; set; }
        public decimal  ISEMIR_KAPANAN { get; set; }
        public decimal  ISEMIR_KALAN { get => ISEMIR_ACILAN - ISEMIR_KAPANAN; }

        static MethodReturn<List<Sason_AracGirisSayilari>> Select_AracGirisSayilari(DateTimeBetween dateTimeBetween, List<decimal> servisIds)
        {
            MethodReturn<List<Sason_AracGirisSayilari>> mr = new MethodReturn<List<Sason_AracGirisSayilari>>(); //user
            using (var ap = new AppPoolMask<SasonWebAppPool>())
            {
                if (ap.AppPool.isNull())
                    ap.AppPool = SasonWebAppPool.Create;
                 mr.Result = ap.AppPool.EbaTestConnector.CreateQuery(@"
                        SELECT
                            tarihler.tarih, servis.id SERVISID, ags.arac_giris, ags.isemir_acilan, ags.isemir_kapanan
                        FROM 
                            (SELECT TARIH FROM TARIHLER WHERE TARIH BETWEEN {startDate} AND {finishDate}) tarihler
                        left join servisler servis on servis.id in (" + servisIds.joinNumeric(",") + @")
                        left join mobilags ags on AGS.SERVIS = servis.id and AGS.TARIH = tarihler.tarih
                        order by tarihler.tarih, servis.id
                    ")
                    .Parameter("startDate", dateTimeBetween.Start.startOfDay())
                    .Parameter("finishDate", dateTimeBetween.Finish.endOfDay())
                    .GetDataTable(mr)
                    .ToModels<Sason_AracGirisSayilari>();
            }
            return mr;
        }

        public static ApiMethodReturn<List<Card>> GetMainCards(DateTime startDate, DateTime finishDate, List<decimal> servisIds)
        {
            MethodReturn<List<Sason_AracGirisSayilari>> mr = Select_AracGirisSayilari(new DateTimeBetween(startDate, finishDate), servisIds);
            ApiMethodReturn<List<Card>> ret = new ApiMethodReturn<List<Card>>();
            if (mr.ok())
            {
                List<Sason_AracGirisSayilari> dataSource = mr.Result.ConvertTo(eTimeGroup.Total);
                Sason_AracGirisSayilari firstItem = dataSource.first();
                ret.Data.add(new Card() { Id = "ARAC_GIRIS", Text = "Araç Giriş", Value = firstItem?.ARAC_GIRIS.ToString(Statics.Nf), Type = "ADET" });
                ret.Data.add(new Card() { Id = "ISEMIR_ACILAN", Text = "Açılan İş Emri", Value = firstItem?.ISEMIR_ACILAN.ToString(Statics.Nf), Type = "ADET" });
                ret.Data.add(new Card() { Id = "ISEMIR_KAPANAN", Text = "Kapanan İş Emri", Value = firstItem?.ISEMIR_KAPANAN.ToString(Statics.Nf), Type = "ADET" });
                ret.Data.forEach(t =>
                {
                    t.HasDetails = true;
                    t.DefaultTimeType = Enum.GetName(typeof(eTimes), eTimes.Last_7_Days);
                    t.Possible = EnumsHelper.EnumZamanTipiAdlari(eTimes.Last_7_Days, eTimes.Last_1_Month, eTimes.Last_6_Months, eTimes.Last_1_Year);
                });
            }
            else
            {
                ret.Exception = mr.ExceptionString;
            }
            return ret;
        }

        public static ApiMethodReturn<Sason_AracGirisSayilari_GraphData> GetCardDetails(eTimes timeType, List<decimal> servisIds)
        {
            MethodReturn<List<Sason_AracGirisSayilari>> mr = Select_AracGirisSayilari(timeType.ToDateTimeBetween(), servisIds);
            ApiMethodReturn<Sason_AracGirisSayilari_GraphData> result = new ApiMethodReturn<Sason_AracGirisSayilari_GraphData>();
            if (mr.ok())
            {
                result.Data = new Sason_AracGirisSayilari_GraphData() { Data = mr.Result.createIsNull().ConvertTo(timeType.ToGroup()) };
            }
            else
            {
                result.Exception = mr.ExceptionString;
            }
            return result;
        }
    }

    public class Sason_AracGirisSayilari_GraphData
    {
        public List<Sason_AracGirisSayilari> Data { get; set; }
    }

    public static class SasonAracGirisSayilariHelper
    {
        public static List<Sason_AracGirisSayilari> ConvertTo(this List<Sason_AracGirisSayilari> source, eTimeGroup gruplama)
        {
            List<Sason_AracGirisSayilari> ret = new List<Sason_AracGirisSayilari>();
            if (source.isEmpty()) return ret;

            Sason_AracGirisSayilari row = null;
            switch (gruplama)
            {
                case eTimeGroup.Day:
                    source.GroupBy(t => t.TARIH).forEach(keyValue =>
                    {
                        row = new Sason_AracGirisSayilari() { Key = keyValue.Key.toLongD(), Text = keyValue.Key.ToString("dd.MM.yyyy") };
                        ret.add(row);
                        keyValue.forEach(value => row.Append(value));
                    });
                    break;
                case eTimeGroup.Week:
                    source.GroupBy(t => $"{t.TARIH.Year}-{t.TARIH.weekOfYear().ToString("00")}").forEach(keyValue =>
                     {
                         row = new Sason_AracGirisSayilari() { Key = keyValue.Key, Text = keyValue.Key };
                         ret.add(row);
                         keyValue.forEach(value => row.Append(value));
                     });
                    break;
                case eTimeGroup.Month:
                    source.GroupBy(t => $"{t.TARIH.Year}-{t.TARIH.Month.ToString("00")}").forEach(keyValue =>
                     {
                         row = new Sason_AracGirisSayilari() { Key = keyValue.Key, Text = keyValue.Key };
                         ret.add(row);
                         keyValue.forEach(value => row.Append(value));
                     });
                    break;
                case eTimeGroup.Year:
                    source.GroupBy(t => t.TARIH.Year).forEach(keyValue =>
                     {
                         row = new Sason_AracGirisSayilari() { Key = keyValue.Key, Text = keyValue.Key.toString() };
                         ret.add(row);
                         keyValue.forEach(value => row.Append(value)); 
                     });
                    break;
                case eTimeGroup.Total:
                    row = new Sason_AracGirisSayilari() { Key = "TOTAL", Text = "TOTAL" };
                    source.forEach(value => row.Append(value));
                    ret.add(row);
                    break;
            }
            return ret;
        }

        public static Sason_AracGirisSayilari Append(this Sason_AracGirisSayilari target, Sason_AracGirisSayilari mobilAGS)
        {
            if (target.isNotNull())
            {
                target.ARAC_GIRIS     += mobilAGS.ARAC_GIRIS;
                target.ISEMIR_ACILAN  += mobilAGS.ISEMIR_ACILAN;
                target.ISEMIR_KAPANAN += mobilAGS.ISEMIR_KAPANAN;
            }
            return target;
        }
    }
}