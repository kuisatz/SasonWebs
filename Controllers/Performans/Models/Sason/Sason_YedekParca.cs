using Antibiotic.Extensions;
using SasonWebs.Models.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SasonWebs.Controllers.Performans.Models.Sason
{
    public class Sason_YedekParca
    {
        public object Key { get; set; }
        public string Text { get; set; }

        public decimal SERVISID { get; set; }
        public DateTime TARIH { get; set; }
        public String KOD { get; set; }
        public Decimal TOPLAM { get; set; }

        static MethodReturn<List<Sason_YedekParca>> Select_YedekParca_Listesi(DateTimeBetween dateTimeBetween, List<decimal> servisIds)
        {
            MethodReturn<List<Sason_YedekParca>> mr = new MethodReturn<List<Sason_YedekParca>>(); //User
            using (var ap = new AppPoolMask<SasonWebAppPool>())
            {
                if (ap.AppPool.isNull())
                    ap.AppPool = SasonWebAppPool.Create;
                mr.Result = ap.AppPool.EbaTestConnector.CreateQuery(@"
                    select 
                        servis.id servisid, tr.tarih, stoktur.kod, sum(pd.tutar) toplam
                    from (select tarih from tarihler where tarih between {startDate} and {finishDate}) tr
                        left join servisler servis on servis.id in ("+ servisIds.joinNumeric(",") + @")
                        left join servisstokturler stoktur on STOKTUR.ID <> -10
                        left join sason.rptable_yedekparcadetay pd on pd.servisid = servis.id and trunc(pd.tarih) = tr.tarih and PD.servisstokturid = stoktur.id
                    group by 
                        servis.id, tr.tarih, stoktur.kod
                    order by
                        servis.id, tr.tarih, stoktur.kod
                    ")
                   .Parameter("startDate", dateTimeBetween.Start.startOfDay())
                   .Parameter("finishDate", dateTimeBetween.Finish.endOfDay())
                   .GetDataTable(mr)
                   .ToModels<Sason_YedekParca>();
            }
            return mr;
        }

        public static ApiMethodReturn<List<Card>> GetMainCards(List<decimal> servisIds)
        {
            ApiMethodReturn<List<Card>> ret = new ApiMethodReturn<List<Card>>();
            MethodReturn<List<Sason_YedekParca>> mr = Select_YedekParca_Listesi(new DateTimeBetween(DateTime.Now.beforeDate().startOfDay(), DateTime.Now.beforeDate().endOfDay()), servisIds);
            if (mr.ok())
            {
                List<Sason_YedekParca> dataSource = mr.Result.ConvertTo(eTimeGroup.Total);
                ret.Data.add(new Card() { Id = "YEDEK_PARCA", Text = "Yedek Parça", Value = dataSource.first()?.TOPLAM.ToString(Statics.Nf), Type = "TL" });
                ret.Data.forEach(t =>
                {
                    t.HasDetails = true;
                    t.DefaultTimeType = Enum.GetName(typeof(eTimes), eTimes.Last_7_Days);
                    t.Possible = EnumsHelper.EnumZamanTipiAdlari(eTimes.Last_7_Days, eTimes.Last_1_Month, eTimes.Last_6_Months);
                });
            }
            else
            {
                ret.Exception = mr.ExceptionString;
            }
            return ret;
        }


        public static ApiMethodReturn<SasonYedekParcaWsDataSource> Select_YedekParca_GraphData(eTimes timeType, List<decimal> servisIds)
        {
            ApiMethodReturn<SasonYedekParcaWsDataSource> ret = new ApiMethodReturn<SasonYedekParcaWsDataSource>();
            MethodReturn<List<Sason_YedekParca>> mr = Select_YedekParca_Listesi(timeType.ToDateTimeBetween(), servisIds);
            if (mr.ok())
            {
                List<SasonYedekParcaData> list = new List<SasonYedekParcaData>();
                mr.Result.GroupBy(t => t.KOD).forEach(keyValue =>
                 {
                     Sason_YedekParca row = new Sason_YedekParca() { Key = keyValue.Key, Text = keyValue.Key };
                     List<Sason_YedekParca> subResults = keyValue.ConvertTo(timeType.ToGroup());
                     subResults.forEach(res =>
                     {
                         SasonYedekParcaData d = new SasonYedekParcaData()
                         {
                             Key = res.Key,
                             Text = keyValue.Key,
                             Value = res.TOPLAM,
                         };
                         list.add(d);
                     });   
                 });


                List<string> texts = new List<string>();

                decimal max = 0;

                List<YedekParcaWsDataRow> retList = new List<YedekParcaWsDataRow>();
                if (list.isNotEmpty())
                {
                    list.GroupBy(t => t.Key).forEach(keyValue =>
                     {
                         YedekParcaWsDataRow rowData = new YedekParcaWsDataRow() { Key = keyValue.Key };
                         retList.add(rowData);
                         keyValue.forEach(subValue =>
                         {
                             rowData.Values.add(subValue.Value);

                             if (subValue.Value > max)
                                 max = subValue.Value;

                             texts.set(subValue.Text);
                         });
                     });
                }

                //ret.Data.MaxValue = max;
                ret.Data.Data = retList;
                ret.Data.ValueTexts = texts;
            }
            else
            {
                ret.Exception = mr.ExceptionString;
            }
            return ret;
        }
    }

    public class SasonYedekParcaWsDataSource
    {
        public List<YedekParcaWsDataRow> Data { get; set; }
        public List<string> ValueTexts { get; set; }
    }

    public class YedekParcaWsDataRow
    {
        public object Key { get; set; }
        public List<decimal> Values { get; set; } = new List<decimal>();
    }


    public class SasonYedekParcaData
    {
        public object Key { get; set; }
        public string Text { get; set; }
        public decimal Value { get; set; }
    }

    public static class SasonYedekParcaHelpers
    {
        public static List<Sason_YedekParca> ConvertTo(this IEnumerable<Sason_YedekParca> source, eTimeGroup gruplama)
        {
            List<Sason_YedekParca> ret = new List<Sason_YedekParca>();
            if (source.isEmpty()) return ret;

            Sason_YedekParca row = null;
            switch (gruplama)
            {
                case eTimeGroup.Day:
                    source.GroupBy(t => t.TARIH.Date).forEach(keyValue =>
                    {
                        row = new Sason_YedekParca() { Key = keyValue.Key.ToString("yyyy-MM-dd"), Text = keyValue.Key.ToString("dd.MM.yyyy") };
                        ret.add(row);
                        keyValue.forEach(value => row.Append(value));
                    });
                    break;
                case eTimeGroup.Week:
                    source.GroupBy(t => $"{t.TARIH.Year}-{t.TARIH.weekOfYear().ToString("00")}").forEach(keyValue =>
                    {
                        row = new Sason_YedekParca() { Key = keyValue.Key, Text = keyValue.Key };
                        ret.add(row);
                        keyValue.forEach(value => row.Append(value));
                    });
                    break;
                case eTimeGroup.Month:
                    source.GroupBy(t => $"{t.TARIH.Year}-{t.TARIH.Month.ToString("00")}").forEach(keyValue =>
                    {
                        row = new Sason_YedekParca() { Key = keyValue.Key, Text = keyValue.Key };
                        ret.add(row);
                        keyValue.forEach(value => row.Append(value));
                    });
                    break;
                case eTimeGroup.Year:
                    source.GroupBy(t => t.TARIH.Year).forEach(keyValue =>
                    {
                        row = new Sason_YedekParca() { Key = keyValue.Key, Text = keyValue.Key.toString() };
                        ret.add(row);
                        keyValue.forEach(value => row.Append(value));
                    });
                    break;
                case eTimeGroup.Total:
                    row = new Sason_YedekParca() { Key = "TOTAL", Text = "TOTAL" };
                    source.forEach(value => row.Append(value));
                    ret.add(row);
                    break;
            }
            return ret;
        }

        public static Sason_YedekParca Append(this Sason_YedekParca target, Sason_YedekParca mobilSatis)
        {
            if (target != null)
            {
                target.TOPLAM += mobilSatis.TOPLAM;
            }
            return target;
        }

    }

}
