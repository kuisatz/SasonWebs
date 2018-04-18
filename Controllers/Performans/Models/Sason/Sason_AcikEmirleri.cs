using Antibiotic.Extensions;
using SasonWebs.Models.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SasonWebs.Controllers.Performans.Models.Sason
{
    public class Sason_AcikEmirleri
    {
        public decimal SERVISID { get; set; }
        public string SERVISADI { get; set; }
        public decimal ACILAN_ISEMIR_SAYISI { get; set; }
        public decimal KAPATILAN_ISEMIR_SAYISI { get; set; }
        public decimal ACIK_ISEMIR_SAYISI { get; set; }

        static MethodReturn<List<Sason_AcikEmirleri>> Select_AcikIsEmirleriSayisi(List<decimal> servisIds)
        {
            MethodReturn<List<Sason_AcikEmirleri>> mr = new MethodReturn<List<Sason_AcikEmirleri>>(); //User
            using (var ap = SasonWebAppPool.CreateMask)
            {
                if (ap.AppPool.isNull())
                    ap.AppPool = SasonWebAppPool.Create;
                mr.Result = ap.AppPool.EbaTestConnector.CreateQuery(@"

                        select max(a) ACIK_ISEMIR_SAYISI, max(b) ACILAN_ISEMIR_SAYISI, max(c) KAPATILAN_ISEMIR_SAYISI, servisid from
                        (
                            select count(id) a, null b, null c, servisid
                            from servisisemirler
                            where servisid in (" + servisIds.joinNumeric(",") + @") and teknikolaraktamamla is null or teknikolaraktamamla = 0 
                            group by servisid

                            union

                            select null a, count(id) b, null c, servisid
                            from servisisemirler
                            where servisid in (" + servisIds.joinNumeric(",") + @")
                            group by servisid

                            union

                            select null a, null b, count(id) c, servisid
                            from servisisemirler
                            where servisid in (" + servisIds.joinNumeric(",") + @") and teknikolaraktamamla = 1
                            group by servisid
                        )
                        group by servisid
                        order by servisid

                    ")
                   .GetDataTable(mr)
                   .ToModels<Sason_AcikEmirleri>();
            }
            return mr;
        }

        public static ApiMethodReturn<List<Sason_AcikEmirleri>> GetAcikIsEmirleri(ServerUserInfo user)
        {
            MethodReturn<List<Sason_AcikEmirleri>> mr = Select_AcikIsEmirleriSayisi(user.ServisIdler);
            ApiMethodReturn<List<Sason_AcikEmirleri>> ret = new ApiMethodReturn<List<Sason_AcikEmirleri>>();
            if (mr.ok())
            {
                List<ServerServisInfo> servisler = Statics.GetServerServisler(user.ServisIdler);// new List<decimal>().add(0));

                servisler.forEach(servis =>
                {
                    Sason_AcikEmirleri foundrow = mr.Result.first(t => t.SERVISID == servis.ServisId);
                    if (foundrow == null)
                    {
                        foundrow = new Sason_AcikEmirleri()
                        {
                            SERVISID = servis.ServisId,
                            SERVISADI = servis.ServisAdi,
                        };
                        mr.Result.add(foundrow);
                    }
                    else
                    {
                        foundrow.SERVISADI = servis.ServisAdi;
                    }
                });

                ret.Data = mr.Result.where(t=> user.ServisIdler.contains(t.SERVISID)).orderBy(t=> t.SERVISID).toList();
            }
            else
            {
                ret.Exception = mr.ExceptionString;
            }
            return ret;
        }

    }
}