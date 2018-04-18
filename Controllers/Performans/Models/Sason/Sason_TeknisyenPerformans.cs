using Antibiotic.Extensions;
using SasonWebs.Models.Api;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static SasonBase.Reports.Teknisyen.VerimlilikReporter;

namespace SasonWebs.Controllers.Performans.Models.Sason
{
    public class Sason_TeknisyenPerformans
    {
        static List<Sason_TeknisyenPerformans_GraphData> Get_TeknisyenPerformansData(List<decimal> servisIdler)
        {
            List<Sason_TeknisyenPerformans_GraphData> ret = new List<Sason_TeknisyenPerformans_GraphData>(); 
            using (var ap = SasonWebAppPool.CreateMask)
            {
                SasonBase.Reports.Teknisyen.VerimlilikReporter reporter = new SasonBase.Reports.Teknisyen.VerimlilikReporter(97, new DateTime(2017, 11, 1), new DateTime(2017, 12, 15));
                List<RptServisTeknisyenInfo> servisTeknisyenler = reporter.ExecuteReport(null).cast<List<RptServisTeknisyenInfo>>();

                servisTeknisyenler.forEach(t =>
                {
                    ret.add(new Sason_TeknisyenPerformans_GraphData()
                    {
                        AdiSoyadi             = $"{t.Teknisyen?.AD} {t.Teknisyen?.SOYAD}",
                        EtkinlikOrani         = t.EtkinlikOrani,
                        KapasiteKullanimOrani = t.KapasiteKullanimOrani,
                        VerimlilikOrani       = t.VerimlilikOrani,
                        FaturalananDakika     = t.FaturalananDakika,
                        FiiliCalisma          = t.FiiliCalisma,
                        NetMevcudiyet         = t.NetMevcudiyet,
                    });
                });
            }
            return ret;
        }

        public static ApiMethodReturn<List<Sason_TeknisyenPerformans_GraphData>> GetTeknisyenGraphData()
        {
            return new ApiMethodReturn<List<Sason_TeknisyenPerformans_GraphData>>() { Data = Get_TeknisyenPerformansData(null) };
        }
    }

    public class Sason_TeknisyenPerformans_GraphData
    {
        public string AdiSoyadi { get; set; }
        public decimal EtkinlikOrani { get; set; }
        public decimal KapasiteKullanimOrani { get; set; }
        public decimal VerimlilikOrani { get; set; }

        public decimal FaturalananDakika { get; set; }
        public decimal FiiliCalisma { get; set; }
        public decimal NetMevcudiyet { get; set; }
    }
}