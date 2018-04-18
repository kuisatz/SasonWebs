using Antibiotic.Extensions;
using SasonBase.Sason.Models.ReportModels;
using SasonBase.Sason.Models.TableModels;
using SasonWebs.Controllers.Performans.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SasonWebs.Controllers.Performans.Helpers
{
    public class ServisCollection : Antibiotic.Collections.Threads.Base.ThreadKeyValueCollectionBase<decimal, ServerServisInfo>
    {
        void CheckServisler()
        {
            //MethodReturn mr = new MethodReturn();
            //FileInfo fi     = new FileInfo("_servisler.json");
            //if (fi.Exists)
            //{
            //    List<ServerServisInfo> list = fi.readAllText().jsonToClass<List<ServerServisInfo>>(mr);
            //    if (mr.ok())
            //    {
            //        list.forEach(x =>
            //        {
            //            base.secretSet(x.ServisId, new ServerServisInfo() { ServisId = x.ServisId, ServisAdi = x.ServisAdi });
            //        });
            //        return;
            //    }
            //}

            using (var apc = SasonWebAppPool.CreateMask)
            {
                List<MOBILSERVISLER> dbServisler = R.Query<MOBILSERVISLER>().OrderBy(t=> t.SERVISID).ToList()
                    .forEach(dbServis =>
                    {
                        base.secretSet(dbServis.SERVISID, new ServerServisInfo() { ServisId = dbServis.SERVISID, ServisAdi = dbServis.SERVISAD });
                    });

                //string servislerJsonData = base.dict.values().toJson();
                //fi.writeToFile(servislerJsonData, mr);
                //if (mr.error())
                //{

                //}
            }
        }

        public List<ServerServisInfo> GetServerServisler(List<decimal> servisIds)
        {
            base.Lock();
            if (base.dict.Count == 0)
                CheckServisler();
            List<ServerServisInfo> ret = new List<ServerServisInfo>();
            if (servisIds.count() == 1 && servisIds.contains(0))
                servisIds = base.dict.keys().remove(1);
            servisIds.forEach(servisId => ret.add(base.secretGet(servisId)));
            base.Unlock();
            return ret;
        }

        public List<ResponseServisInfo> GetResponseServisler(List<decimal> servisIds)
        {
            base.Lock();
            if (base.dict.Count == 0)
                CheckServisler();
            List<ResponseServisInfo> ret = new List<ResponseServisInfo>();
            if (servisIds.count() == 1 && servisIds.contains(0))
                servisIds = base.dict.keys();
            servisIds.forEach(servisId =>
            {
                ServerServisInfo serverServisInfo = base.secretGet(servisId);
                if (serverServisInfo.isNotNull())
                    ret.add(new ResponseServisInfo() { ServisId = servisId, ServisAdi = serverServisInfo.ServisAdi });
            });
            base.Unlock();
            return ret;
        }
    }
}