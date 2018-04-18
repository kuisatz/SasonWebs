using Antibiotic.Extensions;
using SasonWebs.Models.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SasonWebs.Controllers.Performans.Models.Crm.Bayi
{
    public class Bayi_FaturaAdlar
    {
        public decimal ADET { get; set; }
        public string NAME { get; set; }

        static MethodReturn<List<Bayi_FaturaAdlar>> Select_BayiFaturaAdlar(List<decimal> bayiIdler)
        {
            MethodReturn<List<Bayi_FaturaAdlar>> mr = new MethodReturn<List<Bayi_FaturaAdlar>>();
            if (bayiIdler.isNotEmpty() && bayiIdler.Count == 1 && bayiIdler[0] == 0) //Admin
            {
                using (var ap = SasonWebAppPool.CreateMask)
                {
                    mr.Result = ap.AppPool.EbaTestConnector.CreateQuery(@"
                        select count(fb.kay_id) adet,u.name
                        from fatura_bas@crm.oracle fb, users@crm.oracle u
                        where to_char(fb.fatura_tarihi,'YYYY')=to_char(sysdate,'YYYY')
                        and fb.ownergroup=u.user_id
                        group by u.name
                    ")
                    .GetDataTable(mr)
                    .ToModels<Bayi_FaturaAdlar>();
                }
            }
            return mr;
        }

        public static ApiMethodReturn<List<Bayi_FaturaAdlar>> GetBayiFaturalarGraphData(List<decimal> bayiIdler)
        {
            ApiMethodReturn<List<Bayi_FaturaAdlar>> ret = new ApiMethodReturn<List<Bayi_FaturaAdlar>>();
            MethodReturn<List<Bayi_FaturaAdlar>> mr = Select_BayiFaturaAdlar(bayiIdler);
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
