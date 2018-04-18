using Antibiotic.Extensions;
using SasonWebs.Models.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SasonWebs.Controllers.Performans.Models.Crm.Merkez
{
    public class Man_Memnuniyet
    {
        public decimal SERVISID { get; set; }
        /// <summary>
        /// sonuç
        /// </summary>
        public decimal D1 { get; set; }
        public decimal D2 { get; set; }
        public decimal D3 { get; set; }
        public decimal D4 { get; set; }
        public decimal D5 { get; set; }
        public decimal D6 { get; set; }
        public decimal D7 { get; set; }
        public decimal D8 { get; set; }
        public decimal D9 { get; set; }
        public decimal D10 { get; set; }
        public decimal D11 { get; set; }
        public decimal D12 { get; set; }

        /// <summary>
        /// Katılan Kişi Sayısı
        /// </summary>
        public decimal D1BAZ { get; set; }
        public decimal D2BAZ { get; set; }
        public decimal D3BAZ { get; set; }
        public decimal D4BAZ { get; set; }
        public decimal D5BAZ { get; set; }
        public decimal D6BAZ { get; set; }
        public decimal D7BAZ { get; set; }
        public decimal D8BAZ { get; set; }
        public decimal D9BAZ { get; set; }
        public decimal D10BAZ { get; set; }
        public decimal D11BAZ { get; set; }
        public decimal D12BAZ { get; set; }

        public static MethodReturn<List<Man_Memnuniyet>> Select_Memnuniyet_Listesi(DateTimeBetween dateTimeBetween, List<decimal> servisIds)
        {
            MethodReturn<List<Man_Memnuniyet>> mr = new MethodReturn<List<Man_Memnuniyet>>(); //User
            using (var ap = new AppPoolMask<SasonWebAppPool>())
            {
                if (ap.AppPool.isNull())
                    ap.AppPool = SasonWebAppPool.Create;
                mr.Result = ap.AppPool.EbaTestConnector.CreateQuery(@"
                    select * from mobilmusterimemnuniyeti where servisid in (" + servisIds.joinNumeric(",") + @") order by servisid
                    ")
                   .Parameter("startDate", dateTimeBetween.Start.startOfDay())
                   .Parameter("finishDate", dateTimeBetween.Finish.endOfDay())
                   .GetDataTable(mr)
                   .ToModels<Man_Memnuniyet>();
            }
            return mr;
        }

        public static ApiMethodReturn<List<MemnuniyetResult>> GetMemnuniyetResults(List<decimal> servisIds)
        {
            MethodReturn<List<Man_Memnuniyet>> mr = Select_Memnuniyet_Listesi(new DateTimeBetween(), servisIds);
            ApiMethodReturn<List<MemnuniyetResult>> ret = new ApiMethodReturn<List<MemnuniyetResult>>();
            if (mr.ok())
                ret.Data = mr.Result.Convert();
            else
                ret.Exception = mr.ExceptionString;
            return ret;
        }
    }

    public class MemnuniyetResult
    {
        public decimal SERVISID { get; set; }
        public decimal YIL { get; set; }
        public decimal AY { get; set; }
        
        /// <summary>
        /// D1...D12
        /// </summary>
        public decimal ORAN { get; set; }
        /// <summary>
        /// D1BAZ
        /// </summary>
        public decimal KISI_SAYISI { get; set; }
    }


    public static class MemnuniyetHelper
    {
        public static List<MemnuniyetResult> Convert(this List<Man_Memnuniyet> dataSource)
        {
            List<MemnuniyetResult> ret = new List<MemnuniyetResult>();
            dataSource.forEach(ds =>
            {
                1.createBetween(12).forEach(count =>
                {
                    MemnuniyetResult data = new MemnuniyetResult()
                    {
                        SERVISID    = ds.SERVISID,
                        YIL         = 2017,
                        AY          = count,
                        ORAN        = ds.GetPropertyValue<decimal>($"D{count}"),
                        KISI_SAYISI = ds.GetPropertyValue<decimal>($"D{count}BAZ")
                    };
                    ret.add(data);

                });
            });
            return ret;
        }
    }


}