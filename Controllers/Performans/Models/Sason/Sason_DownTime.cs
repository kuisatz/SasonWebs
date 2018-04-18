using Antibiotic.Extensions;
using SasonWebs.Models.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SasonWebs.Controllers.Performans.Models.Sason
{
    public class Sason_DownTime
    {
        public object Key { get; set; }
        public string Text { get; set; }

        public int Gun { get => DOWNTIME.split('.').first().cto<int>(); }
        public int Saat { get => DOWNTIME.split('.').last().cto<int>(); }
        public string DONEM { get; set; }
        public string DOWNTIME { get; set; }
        public decimal DOWNTIMEDEC { get => Convert.ToDecimal($@"{(int)(Gun / 3)},{Saat}"); }

        public static MethodReturn<List<Sason_DownTime>> Select_DownTimes(DateTime startDate, DateTime finishDate, List<decimal> servisIds, eToplamTipi eToplamTipi)
        {
            MethodReturn<List<Sason_DownTime>> mr = new MethodReturn<List<Sason_DownTime>>(); //User
            using (var ap = new AppPoolMask<SasonWebAppPool>())
            {
                if (ap.AppPool.isNull())
                    ap.AppPool = SasonWebAppPool.Create;

                string servisBazindaQuery = "";
                switch (eToplamTipi)
                {
                    case eToplamTipi.ServisBazinda:
                        servisBazindaQuery = ", servisid";
                        break;
                    case eToplamTipi.Toplam:
                        break;
                }

                mr.Result = ap.AppPool.EbaTestConnector.CreateQuery(@"
                        select trunc(round( sum((case when araccikiszamani is not null then araccikiszamani else tamamlanmatarih end ) - kayittarih)/count(id),2)) ||'.'||
                        (trunc((round( sum((case when araccikiszamani is not null then araccikiszamani else tamamlanmatarih end ) - kayittarih)/count(id),2)-
                        trunc(round( sum((case when araccikiszamani is not null then araccikiszamani else tamamlanmatarih end ) - kayittarih)/count(id),2)))*24))||'.' DOWNTIME ,
                          to_char(tamamlanmatarih,'YYYY.MM') DONEM "+ servisBazindaQuery +@"
                        from servisisemirler
                        where tamamlanmatarih between {startDate} and {finishDate} and teknikolaraktamamla=1 and servisid in ("+servisIds.joinNumeric(",") + @")
                        group by to_char(tamamlanmatarih,'YYYY.MM')"+servisBazindaQuery+@"
                        order by to_char(tamamlanmatarih,'YYYY.MM')"+servisBazindaQuery+@"
                    ")
                   .Parameter("startDate", startDate.startOfDay())
                   .Parameter("finishDate", finishDate.endOfDay())
                   .GetDataTable(mr)
                   .ToModels<Sason_DownTime>();
            }
            return mr;
        }

        public static ApiMethodReturn<List<Card>> GetMainCards(DateTime startDate, DateTime finishDate, List<decimal> servisIds)
        {
            ApiMethodReturn<List<Card>> ret = new ApiMethodReturn<List<Card>>();
            MethodReturn<List<Sason_DownTime>> mr = Select_DownTimes(startDate, finishDate, servisIds, eToplamTipi.Toplam);
            if (mr.ok())
            {
                List<Sason_DownTime> dataSource = mr.Result;
                Sason_DownTime firstItem = dataSource.first();
                ret.Data.add(new Card() { Id = "DOWN_TIME", Text = "Araç Kapanış Süresi (Gün)", Value = firstItem?.DOWNTIMEDEC.toString(), Type = "GUN_SAAT" });
            }
            else
            {
                ret.Exception = mr.ExceptionString;
            }
            return ret;
        }
    }
}