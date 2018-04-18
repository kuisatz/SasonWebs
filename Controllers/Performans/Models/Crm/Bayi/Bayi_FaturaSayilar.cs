using Antibiotic.Extensions;
using SasonWebs.Models.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SasonWebs.Controllers.Performans.Models.Crm.Bayi
{
    /// <summary>
    /// Bayi'lerin Aylık Fatura Sayıları
    /// </summary>
    public class Bayi_FaturaSayilar
    {
        public string AYLAR { get; set; }
        public decimal FATURA_EDILEN { get; set; }

        static MethodReturn<List<Bayi_FaturaSayilar>> Select_BayiFaturaSayilar(DateTime start, DateTime finish, List<decimal> bayiIdler)
        {
            MethodReturn<List<Bayi_FaturaSayilar>> mr = new MethodReturn<List<Bayi_FaturaSayilar>>();
            if (bayiIdler.isNotEmpty() && bayiIdler.Count == 1 && bayiIdler[0] == 0) //Admin
            {
                using (var ap = SasonWebAppPool.CreateMask)
                {
                    mr.Result = ap.AppPool.EbaTestConnector.CreateQuery(@"
                        select aylar,count(stok_id) FATURA_EDILEN from (select  to_char(fatura_tarihi,'yyyy/mm') aylar,stok_id
                        from fatura_bas@crm.oracle
                        where fatura_tarihi between {startDate} and {finishDate} 
                        )group by aylar order by aylar
                    ")
                    .Parameter("startDate", start.startOfDay())
                    .Parameter("finishDate", finish.endOfDay())
                    .GetDataTable(mr)
                    .ToModels<Bayi_FaturaSayilar>();
                }
            }
            return mr;
        }

        public static ApiMethodReturn<List<Card>> GetMainCards(List<decimal> bayiIdler)
        {
            ApiMethodReturn<List<Card>> ret = new ApiMethodReturn<List<Card>>();
            MethodReturn<List<Bayi_FaturaSayilar>> mr = Select_BayiFaturaSayilar(DateTime.Now.startOfYear(), DateTime.Now.endOfDay(), bayiIdler);
            if (mr.ok())
            {
                decimal toplamAktifStok = 0;
                List<Bayi_FaturaSayilar> dataSource = mr.Result.createIsNull();
                if (dataSource.isNotEmpty())
                    toplamAktifStok = dataSource.Sum(t => t.FATURA_EDILEN);
                ret.Data.add(new Card() { Id = "BAYI_TOPLAM_FATURALANAN", Text = "Bayiler Fatura Adedi", Value = toplamAktifStok.ToString(Statics.Nf), Type = "ADET" });
                ret.Data.forEach(t =>
                {
                    t.HasDetails = true;
                });
            }
            else
            {
                ret.Exception = mr.ExceptionString;
            }
            return ret;
        }


        public static ApiMethodReturn<List<Bayi_FaturaSayilar>> Select_AylikFaturalananAracSayilari(List<decimal> bayiIdler)
        {
            ApiMethodReturn<List<Bayi_FaturaSayilar>> ret = new ApiMethodReturn<List<Bayi_FaturaSayilar>>();
            MethodReturn<List<Bayi_FaturaSayilar>> mr = Select_BayiFaturaSayilar(DateTime.Now.startOfYear(), DateTime.Now.endOfDay(), bayiIdler);
            if (mr.ok())
            {
                ret.Data = mr.Result.createIsNull();
            }
            else
            {
                ret.Exception = mr.ExceptionString;
            }
            return ret;
        }

    }
}