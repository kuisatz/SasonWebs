using Antibiotic.Extensions;
using SasonWebs.Models.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SasonWebs.Controllers.Performans.Models.Crm.Merkez
{
    public class Man_AracFatura
    {
        public string AYLAR { get; set; }
        public decimal FATURA_EDILEN_ARAC_SAYISI { get; set; }

        public static MethodReturn<List<Man_AracFatura>> Select_ManFaturaSayilar(List<decimal> bayiIdler)
        {
            MethodReturn<List<Man_AracFatura>> mr = new MethodReturn<List<Man_AracFatura>>();
            if (bayiIdler.isNotEmpty() && bayiIdler.Count == 1 && bayiIdler[0] == 0) //Admin
            {
                using (var ap = SasonWebAppPool.CreateMask)
                {
                    mr.Result = ap.AppPool.EbaTestConnector.CreateQuery(@"
                        select aylar,count(product_id) fatura_edilen_arac_sayisi from  (select to_char(fatura_tarihi,'yyyy/mm') aylar,product_id 
                        from stok_master@crm.oracle
                        where fatura_tarihi is not null
                        and product=  1 --APEX_UTIL.GET_PREFERENCE('F114_BT')
                        and fatura_tarihi>last_day(sysdate-430)+1
                        ) group by aylar
                        order by aylar
                    ")
                    .GetDataTable(mr)
                    .ToModels<Man_AracFatura>();
                }
            }
            return mr;
        }

        public static ApiMethodReturn<List<Man_AracFatura>> Select_AylikFaturalananAracSayilari(List<decimal> bayiIdler)
        {
            ApiMethodReturn<List<Man_AracFatura>> ret = new ApiMethodReturn<List<Man_AracFatura>>();
            MethodReturn<List<Man_AracFatura>> mr = Select_ManFaturaSayilar(bayiIdler);
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