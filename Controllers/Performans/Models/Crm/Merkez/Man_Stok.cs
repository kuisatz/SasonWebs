using Antibiotic.Extensions;
using SasonWebs.Models.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static SasonWebs.Controllers.Performans.Models.Crm.Merkez.Man_Stok;

namespace SasonWebs.Controllers.Performans.Models.Crm.Merkez
{
    public class Man_Stok
    {
        public decimal TOPLAM_FATURA { get; set; }
        public decimal FATURA { get; set; }
        public decimal STOK { get; set; }
        public decimal FIKTIF { get; set; }
        public decimal YOLDA { get; set; }
        public decimal TOPLAM_STOK { get; set; }
        public decimal SIPARIS_STOK { get; set; }

        public static MethodReturn<List<Man_Stok>> Select_ManStoklar(List<decimal> bayiIdler)
        {
            MethodReturn<List<Man_Stok>> mr = new MethodReturn<List<Man_Stok>>();
            if (bayiIdler.isNotEmpty() && bayiIdler.Count == 1 && bayiIdler[0] == 0) //Admin
            {
                using (var ap = SasonWebAppPool.CreateMask)
                {
                    mr.Result = ap.AppPool.EbaTestConnector.CreateQuery(@"
                    select '',sum(Toplam_fatura) Toplam_fatura ,sum(fatura_mayis) Fatura, sum(Stok) Stok,
                    sum(Fiktif) Fiktif, sum(Yolda) Yolda , sum(toplam_stok) Toplam_stok , 
                    sum(Siparis_stogu) Siparis_stok
                    from lkw_kontrol_toplam@CRM.ORACLE
                    where product=1
                    ")
                       .GetDataTable(mr)
                       .ToModels<Man_Stok>();
                }
            }
            return mr;
        }

        public static ApiMethodReturn<List<Card>> GetMainCards(List<decimal> bayiIdler)
        {
            ApiMethodReturn<List<Card>> ret = new ApiMethodReturn<List<Card>>();
            MethodReturn<List<Man_Stok>> mr = Select_ManStoklar(bayiIdler);
            if (mr.ok())
            {
                List<Man_Stok> dataSource = mr.Result.createIsNull();
                ret.Data.add(new Card() { Id = "MAN_STOK_TOPLAM_FATURALANAN", Text = "Faturalanan Satış", Value = dataSource.first()?.TOPLAM_FATURA.ToString(Statics.Nf), Type = "ADET" });
                ret.Data.add(new Card() { Id = "MAN_TOPLAM_STOK", Text = "Toplam Stok", Value = dataSource.first()?.TOPLAM_STOK.ToString(Statics.Nf), Type = "ADET" });
                ret.Data.forEach(t =>
                {
                    //t.HasDetails = true;
                });
            }
            else
            {
                ret.Exception = mr.ExceptionString;
            }
            return ret;
        }


        public static ApiMethodReturn<List<ManStokData>> Select_ManStokData(List<decimal> bayiIdler)
        {
            MethodReturn<List<Man_Stok>> mr = Select_ManStoklar(bayiIdler);
            ApiMethodReturn<List<ManStokData>> ret = new ApiMethodReturn<List<ManStokData>>();
            if (mr.ok())
            {
                ret.Data = mr.Result.ConvertToGraphData();
            }
            else
            {
                ret.Exception = mr.ExceptionString;
            }
            return ret;
        }

        public class ManStokData
        {
            public string Key { get; set; }
            public string Text { get; set; }
            public decimal Value { get; set; }
        }
    }

    public static class ManStokHelper
    {
        public static List<ManStokData> ConvertToGraphData(this List<Man_Stok> source)
        {
            List<ManStokData> ret = new List<ManStokData>();
            if (source.isEmpty()) return ret;

            List<string> keys = new List<string>().addRange("TOPLAM_FATURA", "FATURA", "STOK", "FIKTIF", "YOLDA", "TOPLAM_STOK", "SIPARIS_STOK");
            List<string> texts = new List<string>().addRange("Faturalanan", "Bu Ay Faturalanan", "Stok", "Fiktif", "Yolda", "Toplam Stok","Siparis Stok");
            int keyIndex = 0;
            keys.forEach(key =>
            {
                ManStokData manStokData = new ManStokData()
                {
                    Key = key,
                    Text = texts.value(keyIndex)
                };

                source.forEach(data =>
                {
                    manStokData.Value = data.GetPropertyValue<decimal>(key);
                });

                ret.add(manStokData);
                keyIndex++;
            });
            return ret;
        }
    }
}