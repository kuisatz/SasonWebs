using Antibiotic.Extensions;
using SasonWebs.Models.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SasonWebs.Controllers.Performans.Models.Sason
{
    public class Sason_ServislerCiro
    {
        public object Key { get; set; }
        public string Text { get; set; }

        public DateTime TARIH { get; set; }
        public decimal SERVISID { get; set; }
        public decimal GUNLUK_CIRO { get; set; }


        static MethodReturn<List<Sason_ServislerCiro>> Select_Cirolar(DateTimeBetween dateTimeBetween, List<decimal> servisIds)
        {
            MethodReturn<List<Sason_ServislerCiro>> mr = new MethodReturn<List<Sason_ServislerCiro>>(); //User
            using (var ap = new AppPoolMask<SasonWebAppPool>())
            {
                if (ap.AppPool.isNull())
                    ap.AppPool = SasonWebAppPool.Create;
                mr.Result = ap.AppPool.EbaTestConnector.CreateQuery(@"
                        select sum(toplamamk)gunluk_ciro,servisid,tarih
                        from (
                        select sum(toplam) toplamamk ,servisid,trunc(islemtarihi) tarih,faturaturid
                        from faturalar
                        where  durumid=1 and faturaturid in(1,2,3)
                        group by servisid,trunc(islemtarihi),faturaturid
                        order by servisid
                        ) a
                        where tarih between {startDate} and {finishDate} and servisid in ("+servisIds.joinNumeric(",")+@")
                        group by servisid,tarih
                        order by tarih
                    ")
                   .Parameter("startDate", dateTimeBetween.Start.startOfDay())
                   .Parameter("finishDate", dateTimeBetween.Finish.endOfDay())
                   .GetDataTable(mr)
                   .ToModels<Sason_ServislerCiro>();
            }
            return mr;
        }

        public static ApiMethodReturn<List<Card>> GetMainCards(DateTime startDate, DateTime finishDate, List<decimal> servisIds)
        {
            ApiMethodReturn<List<Card>> ret = new ApiMethodReturn<List<Card>>();
            MethodReturn<List<Sason_ServislerCiro>> mr = Select_Cirolar(new DateTimeBetween(startDate, finishDate), servisIds);
            if (mr.ok())
            {
                List<Sason_ServislerCiro> dataSource = mr.Result.ConvertTo(eTimeGroup.Total);
                Sason_ServislerCiro firstItem = dataSource.first().createIsNull();
                ret.Data.add(new Card() { Id = "CIROLAR", Text = "Ciro", Value = firstItem.GUNLUK_CIRO.ToString(Statics.Nf), Type = "TL" });
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

        public static ApiMethodReturn<List<Sason_ServislerCiro>> GetCirolarDetay(eTimes timeType, List<decimal> servisIds)
        {
            ApiMethodReturn<List<Sason_ServislerCiro>> ret = new ApiMethodReturn<List<Sason_ServislerCiro>>();
            MethodReturn<List<Sason_ServislerCiro>> mr = Select_Cirolar(timeType.ToDateTimeBetween(), servisIds);
            if (mr.ok())
            {
                ret.Data = mr.Result.createIsNull().ConvertTo(timeType.ToGroup());
            }
            else
            {
                ret.Exception = mr.ExceptionString;
            }
            return ret;
        }

    }



    public static class ServislerCiroHelper
    {
        public static List<Sason_ServislerCiro> ConvertTo(this List<Sason_ServislerCiro> source, eTimeGroup gruplama)
        {
            List<Sason_ServislerCiro> ret = new List<Sason_ServislerCiro>();
            if (source.isEmpty()) return ret;

            Sason_ServislerCiro row = null;
            switch (gruplama)
            {
                case eTimeGroup.Day:
                    source.GroupBy(t => t.TARIH).forEach(keyValue =>
                    {
                        row = new Sason_ServislerCiro() { Key = keyValue.Key.toLongD(), Text = keyValue.Key.ToString("dd.MM.yyyy") };
                        ret.add(row);
                        keyValue.forEach(value => row.Append(value));
                    });
                    break;
                case eTimeGroup.Week:
                    source.GroupBy(t => $"{t.TARIH.Year}-{t.TARIH.weekOfYear().ToString("00")}").forEach(keyValue =>
                    {
                        row = new Sason_ServislerCiro() { Key = keyValue.Key, Text = keyValue.Key };
                        ret.add(row);
                        keyValue.forEach(value => row.Append(value));
                    });
                    break;
                case eTimeGroup.Month:
                    source.GroupBy(t => $"{t.TARIH.Year}-{t.TARIH.Month.ToString("00")}").forEach(keyValue =>
                    {
                        row = new Sason_ServislerCiro() { Key = keyValue.Key, Text = keyValue.Key };
                        ret.add(row);
                        keyValue.forEach(value => row.Append(value));
                    });
                    break;
                case eTimeGroup.Year:
                    source.GroupBy(t => t.TARIH.Year).forEach(keyValue =>
                    {
                        row = new Sason_ServislerCiro() { Key = keyValue.Key, Text = keyValue.Key.toString() };
                        ret.add(row);
                        keyValue.forEach(value => row.Append(value));
                    });
                    break;
                case eTimeGroup.Total:
                    row = new Sason_ServislerCiro() { Key = "TOTAL", Text = "TOTAL" };
                    source.forEach(value => row.Append(value));
                    ret.add(row);
                    break;
            }
            return ret;
        }


        public static Sason_ServislerCiro Append(this Sason_ServislerCiro target, Sason_ServislerCiro ciro)
        {
            if (target != null)
            {
                target.GUNLUK_CIRO += ciro.GUNLUK_CIRO;
            }
            return target;
        }
    }
}