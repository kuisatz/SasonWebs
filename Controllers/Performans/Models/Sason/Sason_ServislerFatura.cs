using Antibiotic.Extensions;
using SasonWebs.Models.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SasonWebs.Controllers.Performans.Models.Sason
{
    public class Sason_ServislerFatura
    {
        public object Key { get; set; }
        public string Text { get; set; }

        public DateTime TARIH { get; set; }
        public decimal SERVISID { get; set; }
        public decimal ALISFATURASI { get; set; }
        public decimal ISEMRIFATURASI { get; set; }
        public decimal SATISFATURASI { get; set; }
        public decimal ICMALFATURASI { get; set; }
        public decimal DISHIZMETFATURASI { get; set; }

        static MethodReturn<List<Sason_ServislerFatura>> Select_Faturalar(DateTimeBetween dateTimeBetween, List<decimal> servisIds)
        {
            MethodReturn<List<Sason_ServislerFatura>> mr = new MethodReturn<List<Sason_ServislerFatura>>(); //User
            using (var ap = new AppPoolMask<SasonWebAppPool>())
            {
                if (ap.AppPool.isNull())
                    ap.AppPool = SasonWebAppPool.Create;
                mr.Result = ap.AppPool.EbaTestConnector.CreateQuery(@"
                    select 
                        tarihler.tarih tarih, sv.id servisid, mf.* 
                    from (select tarih from tarihler where tarih between {startDate} and {finishDate}) tarihler
                        left join servisler sv on sv.id in (" + servisIds.joinNumeric(",") + @")
                        left join mobilfatura mf on mf.servisid = sv.id and mf.tarih = tarihler.tarih
                    order by tarihler.tarih, sv.id
                    ")
                   .Parameter("startDate", dateTimeBetween.Start.startOfDay())
                   .Parameter("finishDate", dateTimeBetween.Finish.endOfDay())
                   .GetDataTable(mr)
                   .ToModels<Sason_ServislerFatura>();
            }
            return mr;
        }

        public static ApiMethodReturn<List<Card>> GetMainCards(List<decimal> servisIds)
        {
            ApiMethodReturn<List<Card>> ret = new ApiMethodReturn<List<Card>>();

            DateTime start = DateTime.Now.startOfDay();
            DateTime finish = DateTime.Now.endOfDay();
#if DEBUG
            start = DateTime.Now.beforeDate().startOfDay();
#endif

            MethodReturn<List<Sason_ServislerFatura>> mr = Select_Faturalar(new DateTimeBetween(DateTime.Now.startOfDay(), DateTime.Now.endOfDay()), servisIds);
            if (mr.ok())
            {
                List<Sason_ServislerFatura> dataSource = mr.Result.ConvertTo(eTimeGroup.Total);
                Sason_ServislerFatura firstItem = dataSource.first();
                ret.Data.add(new Card() { Id = "ALIS_FATURALARI", Text = "Alış Faturaları", Value = firstItem?.ALISFATURASI.RoundToDecimals(2).ToString(Statics.Nf), Type = "TL" });
                ret.Data.add(new Card() { Id = "ISEMRI_FATURALARI", Text = "İş Emri Faturaları", Value = firstItem?.ISEMRIFATURASI.RoundToDecimals(2).ToString(Statics.Nf), Type="TL" });
                ret.Data.add(new Card() { Id = "SATIS_FATURALARI", Text = "Satış Faturaları", Value = firstItem?.SATISFATURASI.RoundToDecimals(2).ToString(Statics.Nf), Type = "TL" });
                ret.Data.add(new Card() { Id = "ICMAL_FATURALARI", Text = "İcmal Faturaları", Value = firstItem?.ICMALFATURASI.RoundToDecimals(2).ToString(Statics.Nf), Type = "TL" });
                ret.Data.add(new Card() { Id = "DISHIZMET_FATURALARI", Text = "Dış Hizmet Faturaları", Value = firstItem?.DISHIZMETFATURASI.RoundToDecimals(2).ToString(Statics.Nf), Type = "TL" });

                ret.Data.forEach(t         => {
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

        public static ApiMethodReturn<ServislerFatura_GraphData> GetCardDetails(eTimes timeType, List<decimal> servisIds)
        {
            MethodReturn<List<Sason_ServislerFatura>> mr = Select_Faturalar(timeType.ToDateTimeBetween(), servisIds);
            ApiMethodReturn<ServislerFatura_GraphData> result = new ApiMethodReturn<ServislerFatura_GraphData>();
            if (mr.ok())
            {
                result.Data = new ServislerFatura_GraphData() { Data = mr.Result.createIsNull().ConvertTo(timeType.ToGroup()) };
            }
            else
            {
                result.Exception = mr.ExceptionString;
            }
            return result;
        }
    }

    public class ServislerFatura_GraphData
    {
        public List<Sason_ServislerFatura> Data { get; set; }
    }


    public static class ServislerFaturaHelper
    {
        public static List<Sason_ServislerFatura> ConvertTo(this List<Sason_ServislerFatura> source, eTimeGroup gruplama)
        {
            List<Sason_ServislerFatura> ret = new List<Sason_ServislerFatura>();
            if (source.isEmpty()) return ret;

            Sason_ServislerFatura row = null;
            switch (gruplama)
            {
                case eTimeGroup.Day:
                    source.GroupBy(t => t.TARIH).forEach(keyValue =>
                    {
                        row = new Sason_ServislerFatura() { Key = keyValue.Key.toLongD(), Text = keyValue.Key.ToString("yyyy-MM-dd") };
                        ret.add(row);
                        keyValue.forEach(value => row.Append(value));
                    });
                    break;
                case eTimeGroup.Week:
                    source.GroupBy(t => $"{t.TARIH.Year}-{t.TARIH.weekOfYear().ToString("00")}").forEach(keyValue =>
                    {
                        row = new Sason_ServislerFatura() { Key = keyValue.Key, Text = keyValue.Key };
                        ret.add(row);
                        keyValue.forEach(value => row.Append(value));
                    });
                    break;
                case eTimeGroup.Month:
                    source.GroupBy(t => $"{t.TARIH.Year}-{t.TARIH.Month.ToString("00")}").forEach(keyValue =>
                    {
                        row = new Sason_ServislerFatura() { Key = keyValue.Key, Text = keyValue.Key };
                        ret.add(row);
                        keyValue.forEach(value => row.Append(value));
                    });
                    break;
                case eTimeGroup.Year:
                    source.GroupBy(t => t.TARIH.Year).forEach(keyValue =>
                    {
                        row = new Sason_ServislerFatura() { Key = keyValue.Key, Text = keyValue.Key.toString() };
                        ret.add(row);
                        keyValue.forEach(value => row.Append(value));
                    });
                    break;
                case eTimeGroup.Total:
                    row = new Sason_ServislerFatura() { Key = "TOTAL", Text = "TOTAL" };
                    source.forEach(value => row.Append(value));
                    ret.add(row);
                    break;
            }
            return ret;
        }

        public static Sason_ServislerFatura Append(this Sason_ServislerFatura target, Sason_ServislerFatura mobilFaturalar)
        {
            if (target != null)
            {
                target.ALISFATURASI      += mobilFaturalar.ALISFATURASI       ;
                target.ISEMRIFATURASI    += mobilFaturalar.ISEMRIFATURASI     ;
                target.SATISFATURASI     += mobilFaturalar.SATISFATURASI      ;
                target.ICMALFATURASI     += mobilFaturalar.ICMALFATURASI      ;
                target.DISHIZMETFATURASI += mobilFaturalar.DISHIZMETFATURASI;
            }
            return target;
        }

    }
}