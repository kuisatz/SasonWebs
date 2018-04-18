using Antibiotic.Extensions;
using SasonWebs.Models.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SasonWebs.Controllers.Performans.Models.Crm.Merkez
{
    public class Man_SalesFunnelKritik
    {
        public object Key { get; set; }
        public string Text { get; set; }

        public decimal KGTPA { get; set; }
        public decimal TEGTTA { get; set; }
        public decimal YGTPA { get; set; }
        public decimal TGTAA { get; set; }
        public decimal OWNERGROUP { get; set; }

        public static MethodReturn<List<Man_SalesFunnelKritik>> Select_SalesFunnel_Kritik(List<decimal> bayiIdler)
        {
            MethodReturn<List<Man_SalesFunnelKritik>> mr = new MethodReturn<List<Man_SalesFunnelKritik>>();
            if (bayiIdler.isNotEmpty() && bayiIdler.Count == 1 && bayiIdler[0] == 0) //Admin
            {
                using (var ap = new AppPoolMask<SasonWebAppPool>())
                {
                    if (ap.AppPool.isNull())
                        ap.AppPool = SasonWebAppPool.Create;
                    mr.Result = ap.AppPool.EbaTestConnector.CreateQuery(@"
                        select 
                            round(((select y_retail_satis_h from app_sales_funnel@crm.oracle ) /PBAA)) KGTPA,
                            round((((select y_retail_satis_h from app_sales_funnel@crm.oracle) /PBAA) /PKO)) TEGTTA,
                            round(((((select y_retail_satis_h from app_sales_funnel@crm.oracle) /PBAA) /PKO)/OTO))  YGTPA,
                            round((((((select y_retail_satis_h from app_sales_funnel@crm.oracle) /PBAA) /PKO)/OTO)/POO))TGTAA,
                            ownergroup
                        from funnel_kritik_olcum@crm.oracle
                        where PBAA<>0 and ownergroup in ("+bayiIdler.joinNumeric(",")+@")
                    ")
                    .GetDataTable(mr)
                    .ToModels<Man_SalesFunnelKritik>();
                }
            }
            return mr;
        }

        public static ApiMethodReturn<List<Man_SalesFunnelKritik_GraphData>> Select_SalesFunnel_Kritik_WsData(List<decimal> bayiIdler)
        {
            MethodReturn<List<Man_SalesFunnelKritik>> mr = Select_SalesFunnel_Kritik(bayiIdler);
            ApiMethodReturn<List<Man_SalesFunnelKritik_GraphData>> result = new ApiMethodReturn<List<Man_SalesFunnelKritik_GraphData>>();
            if (mr.ok())
            {
                result.Data = mr.Result.createIsNull().ConvertToWsData();
            }
            else
            {
                result.Exception = mr.ExceptionString;
            }
            return result;
        }
    }

    public class Man_SalesFunnelKritik_GraphData
    {
        public string Key { get; set; }
        public string Text { get; set; }
        public decimal Value { get; set; }
    }

    public static class ManSalesFunnelKritikHelper
    {
        public static List<Man_SalesFunnelKritik_GraphData> ConvertToWsData(this List<Man_SalesFunnelKritik> source)
        {
            List<Man_SalesFunnelKritik_GraphData> ret = new List<Man_SalesFunnelKritik_GraphData>();
            if (source.isEmpty()) return ret;

            List<string> keys = new List<string>().addRange("KGTPA", "TEGTTA", "YGTPA", "TGTAA");
            List<string> texts = new List<string>().addRange("Kazanılması Gereken Toplam Proje Sayısı", "Teklif Edilmesi Gereken Toplam Teklif Sayısı", "Yaratılması Gereken Toplam Proje Sayısı", "Tamamlanması Gereken Toplam Aktivite Sayısı");
            int keyIndex = 0;
            keys.forEach(key =>
            {
                Man_SalesFunnelKritik_GraphData keyData = new Man_SalesFunnelKritik_GraphData() { Key = key, Text = texts.value(keyIndex) };
                source.forEach(data => keyData.Value = data.GetPropertyValue<decimal>(key));
                ret.add(keyData);
                keyIndex++;
            });
            return ret;
        }
    }
}
