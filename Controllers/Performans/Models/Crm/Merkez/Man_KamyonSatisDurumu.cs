using Antibiotic.Extensions;
using SasonWebs.Models.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SasonWebs.Controllers.Performans.Models.Crm.Merkez
{
    public class Man_KamyonSatisDurumu
    {
        public decimal SIRA { get; set; }
        public string ACK { get; set; }
        public decimal ADET { get; set; }

        static MethodReturn<List<Man_KamyonSatisDurumu>> Select_SatisDurumlari(List<decimal> bayiIdler)
        {
            MethodReturn<List<Man_KamyonSatisDurumu>> mr = new MethodReturn<List<Man_KamyonSatisDurumu>>();
            if (bayiIdler.isNotEmpty() && bayiIdler.Count == 1 && bayiIdler[0] == 0) //Admin
            {
                using (var ap = new AppPoolMask<SasonWebAppPool>())
                {
                    if (ap.AppPool.isNull())
                        ap.AppPool = SasonWebAppPool.Create;
                    mr.Result = ap.AppPool.EbaTestConnector.CreateQuery(@"
                        select * from mobilkart@crm.oracle order by sira                   
                        ")
                       .GetDataTable(mr)
                       .ToModels<Man_KamyonSatisDurumu>();
                }
            }
            return mr;
        }

        public static ApiMethodReturn<List<Man_KamyonSatisDurumu_GridData>> GetSatisDurumlari(List<decimal> bayiIdler)
        {
            MethodReturn<List<Man_KamyonSatisDurumu>> mr = Select_SatisDurumlari(bayiIdler);
            ApiMethodReturn<List<Man_KamyonSatisDurumu_GridData>> ret = new ApiMethodReturn<List<Man_KamyonSatisDurumu_GridData>>();
            if (mr.ok())
            {
                mr.Result.forEach(data =>
                {
                    ret.Data.add(new Man_KamyonSatisDurumu_GridData()
                    {
                        Key = data.ACK.Replace(" ", "").Replace(">","").Replace("-","").Replace("<","").removeTrChars(),
                        Value = data.ADET,
                    });
                });
            }
            else
                ret.Exception = mr.ExceptionString;
            return ret;
        }

    }

    public class Man_KamyonSatisDurumu_GridData
    {
        public string Key { get; set; }
        public decimal Value { get; set; }
    }

}