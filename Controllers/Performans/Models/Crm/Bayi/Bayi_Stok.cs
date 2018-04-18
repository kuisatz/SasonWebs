using Antibiotic.Extensions;
using SasonWebs.Models.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SasonWebs.Controllers.Performans.Models.Crm.Bayi
{
    public class Bayi_Stok
    {
        public string BAYI { get; set; }
        public decimal KAMYON_STOK_ADEDI { get; set; }
        public decimal BAYIID { get; set; }

        static MethodReturn<List<Bayi_Stok>> Select_BayiStoklar(List<decimal> bayiIdler)
        {
            MethodReturn<List<Bayi_Stok>> mr = new MethodReturn<List<Bayi_Stok>>(); //User
            if (bayiIdler.isEmpty())
                return mr;
            using (var ap = SasonWebAppPool.CreateMask)
            {
                string bayiIdQuery = "";
                if (bayiIdler.Count >= 1)
                {
                    bayiIdQuery = @" and u.user_id IN (" + bayiIdler.joinNumeric(",") + @") ";
                    if (bayiIdler.Count == 1 && bayiIdler[0] == 0)
                        bayiIdQuery = "";
                }
                mr.Result = ap.AppPool.EbaTestConnector.CreateQuery(@"
                    SELECT u.username Bayi,count(stok_id) kamyon_stok_adedi, u.user_id bayiid
                    FROM STOK_MASTER@crm.oracle sm,users@crm.oracle u 
                    WHERE sm.bayi=u.user_id
                    and u.user_id not in (117,120,234,113,230,0,112)
                    AND sm.bayi is not null
                    and ((sm.tahsisat_durumu=4 AND sm.DURUM<>6) OR (sm.tahsisat_durumu=3 AND sm.DURUM=5)) and u.product=1 and u.aktifmi=1
                    "+bayiIdQuery+@"
                    group by u.username, u.user_id
                    order by count(stok_id) desc
                    ")
                   .GetDataTable(mr)
                   .ToModels<Bayi_Stok>();
            }
            return mr;
        }

        public static ApiMethodReturn<List<Card>> GetMainCards(List<decimal> bayiIdler)
        {
            ApiMethodReturn<List<Card>> ret = new ApiMethodReturn<List<Card>>();
            MethodReturn<List<Bayi_Stok>> mr = Select_BayiStoklar(bayiIdler);
            if (mr.ok())
            {
                decimal toplamAktifStok = 0;
                List<Bayi_Stok> dataSource = mr.Result.createIsNull();
                if (dataSource.isNotEmpty())
                    toplamAktifStok = dataSource.Sum(t => t.KAMYON_STOK_ADEDI);
                ret.Data.add(new Card() { Id = "BAYI_STOKLAR", Text = "Bayi Stoklar", Value = toplamAktifStok.ToString(Statics.Nf), Type = "ADET" });
                ret.Data.forEach(t =>
                {
                    t.HasDetails = true;
                    t.DefaultTimeType = Enum.GetName(typeof(eTimes), eTimes.Now);
                    t.Possible = EnumsHelper.EnumZamanTipiAdlari(eTimes.Now);
                });
            }
            else
            {
                ret.Exception = mr.ExceptionString;
            }
            return ret;
        }

        public static ApiMethodReturn<List<Bayi_Stok_GraphData>> GetBayiStokGraphData(List<decimal> bayiIdler)
        {
            ApiMethodReturn<List<Bayi_Stok_GraphData>> ret = new ApiMethodReturn<List<Bayi_Stok_GraphData>>();
            MethodReturn<List<Bayi_Stok>> mr = Select_BayiStoklar(bayiIdler);
            if (mr.ok())
            {
                mr.Result.createIsNull().forEach(data =>
                {
                    ret.Data.add(new Bayi_Stok_GraphData()
                    {
                        Text = data.BAYI,
                        Value = data.KAMYON_STOK_ADEDI,
                    });
                });
            }
            else
            {
                ret.Exception = mr.ExceptionString;
            }
            return ret;
        }
    }

    public class Bayi_Stok_GraphData
    {
        public string Text { get; set; }
        public decimal Value { get; set; }
    }
}