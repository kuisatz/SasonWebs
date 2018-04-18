using Antibiotic.Extensions;
using SasonWebs.Models.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SasonWebs.Controllers.Performans.Models.Sason
{
    public class Sason_AcikIsEmirleri_Ozet
    {
        public decimal ACIK_ISEMIRLERI_SAYISI { get; set; }

        static MethodReturn<List<Sason_AcikIsEmirleri_Ozet>> Select_AcikIsEmirleriSayisi(List<decimal> servisIds)
        {
            MethodReturn<List<Sason_AcikIsEmirleri_Ozet>> mr = new MethodReturn<List<Sason_AcikIsEmirleri_Ozet>>(); //user
            using (var ap = SasonWebAppPool.CreateMask)
            {
                if (ap.AppPool.isNull())
                    ap.AppPool = SasonWebAppPool.Create;
                mr.Result = ap.AppPool.EbaTestConnector.CreateQuery(@"
                        select count(*) ACIK_ISEMIRLERI_SAYISI from servisisemirler where servisid in ("+ servisIds.joinNumeric(",") +@") and teknikolaraktamamla = 0 or teknikolaraktamamla is null
                    ")
                   .GetDataTable(mr)
                   .ToModels<Sason_AcikIsEmirleri_Ozet>();
            }
            return mr;
        }

        public static ApiMethodReturn<List<Card>> GetMainCards(ServerUserInfo user)
        {
            MethodReturn<List<Sason_AcikIsEmirleri_Ozet>> mr = Select_AcikIsEmirleriSayisi(user.ServisIdler);
            ApiMethodReturn<List<Card>> ret = new ApiMethodReturn<List<Card>>();
            if (mr.ok())
            {
                Sason_AcikIsEmirleri_Ozet firstItem = mr.Result.first();
                ret.Data.add(new Card() { Id = "ACIK_ISEMIRLERI", Text = "Açık İş Emirleri", Value = firstItem?.ACIK_ISEMIRLERI_SAYISI.ToString(Statics.Nf), Type = "ADET" });
                ret.Data.forEach(t =>
                {
                    t.HasDetails = true;
                    t.DefaultTimeType = Enum.GetName(typeof(eTimes), eTimes.Now);
                });
            }
            else
            {
                ret.Exception = mr.ExceptionString;
            }
            return ret;
        }
    }
}
