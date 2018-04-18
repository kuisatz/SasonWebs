using Antibiotic.Extensions;
using SasonWebs.Models.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SasonWebs.Controllers.Performans.Models.Crm.Merkez
{
    public class Man_SalesFunnel
    {
        public object Key { get; set; }
        public string Text { get; set; }

        public decimal Y_RVP_TOPLAM_KAMYON { get; set; }
        public decimal Y_KAMU_SATIS_H { get; set; }
        public decimal Y_IKA_SATIS_H { get; set; }
        public decimal EY_BAYI_STOK { get; set; }
        public decimal YS_BAYI_STOK_H { get; set; }
        public decimal Y_RETAIL_SATIS_H { get; set; }
        public decimal YEAR { get; set; }
        public decimal ID { get; set; }

        internal static MethodReturn<List<Man_SalesFunnel>> Select_Sales_Funnel(DateTimeBetween dateTimeBetween, List<decimal> bayiIdler)
        {
            MethodReturn<List<Man_SalesFunnel>> mr = new MethodReturn<List<Man_SalesFunnel>>();
            if (bayiIdler.isNotEmpty() && bayiIdler.Count == 1 && bayiIdler[0] == 0) //Admin
            {
                using (var ap = new AppPoolMask<SasonWebAppPool>())
                {
                    if (ap.AppPool.isNull())
                        ap.AppPool = SasonWebAppPool.Create;
                    mr.Result = ap.AppPool.EbaTestConnector.CreateQuery(@"
                        select * from app_sales_funnel@crm.oracle where year between {startYear} and {finishYear}
                    ")
                       .Parameter("startYear", dateTimeBetween.Start.Year)
                       .Parameter("finishYear", dateTimeBetween.Finish.Year)
                       .GetDataTable(mr)
                       .ToModels<Man_SalesFunnel>();
                }
            }
            return mr;
        }

        public static ApiMethodReturn<List<Man_SalesFunnel_GraphData>> Select_SalesFunnel_WsData(eTimes timeType, List<decimal> bayiIdler)
        {
            MethodReturn<List<Man_SalesFunnel>> mr = Select_Sales_Funnel(timeType.ToDateTimeBetween(), bayiIdler);
            ApiMethodReturn<List<Man_SalesFunnel_GraphData>> result = new ApiMethodReturn<List<Man_SalesFunnel_GraphData>>();
            if (mr.ok())
            {
                result.Data = mr.Result.createIsNull().ConvertTo(timeType.ToGroup()).ConvertToWsData();
            }
            else
            {
                result.Exception = mr.ExceptionString;
            }
            return result;
        }
    }



    public class Man_SalesFunnel_GraphData
    {
        public string Key { get; set; }
        public string Text { get; set; }
        public decimal Value { get; set; }
    }

    public static class ManSalesFunnelHelper
    {
        public static List<Man_SalesFunnel_GraphData> ConvertToWsData(this List<Man_SalesFunnel> source)
        {
            List<Man_SalesFunnel_GraphData> ret = new List<Man_SalesFunnel_GraphData>();
            if (source.isEmpty()) return ret;

            List<string> keys = new List<string>().addRange("Y_RVP_TOPLAM_KAMYON", "Y_KAMU_SATIS_H", "Y_IKA_SATIS_H", "EY_BAYI_STOK", "YS_BAYI_STOK_H", "Y_RETAIL_SATIS_H");
            List<string> texts = new List<string>().addRange("RVP Toplam Kamyon Satış Sayısı", "KAMU Satış Hedef Sayısı", "IKA Satış Hedef Sayısı", "Bir Önceki Yılın Bayi Stok Sayısı", "Yıl Sonu Bayi Stok Hedef Sayısı", "Yıllık RETAIL Satış Hedef");
            int keyIndex = 0;
            keys.forEach(key =>
            {
                Man_SalesFunnel_GraphData keyData = new Man_SalesFunnel_GraphData() { Key = key, Text = texts.value(keyIndex) };
                source.forEach(data => keyData.Value = data.GetPropertyValue<decimal>(key));
                ret.add(keyData);
                keyIndex++;
            });
            return ret;
        }

        public static List<Man_SalesFunnel> ConvertTo(this List<Man_SalesFunnel> source, eTimeGroup gruplama)
        {
            List<Man_SalesFunnel> ret = new List<Man_SalesFunnel>();
            if (source.isEmpty()) return ret;

            Man_SalesFunnel row = null;
            switch (gruplama)
            {
                case eTimeGroup.Day:
                    break;
                case eTimeGroup.Week:
                    break;
                case eTimeGroup.Month:
                    source.GroupBy(t => t.YEAR).forEach(keyValue =>
                    {
                        row = new Man_SalesFunnel() { Key = keyValue.Key, Text = keyValue.Key.toString() };
                        ret.add(row);
                        keyValue.forEach(value => row.Append(value));
                    });
                    break;
                case eTimeGroup.Year:
                    break;
                case eTimeGroup.Total:
                    break;
            }
            return ret;
        }

        public static Man_SalesFunnel Append(this Man_SalesFunnel target, Man_SalesFunnel funnel)
        {
            if (target != null)
            {
                target.Y_RVP_TOPLAM_KAMYON += funnel.Y_RVP_TOPLAM_KAMYON;
                target.Y_KAMU_SATIS_H      += funnel.Y_KAMU_SATIS_H;
                target.Y_IKA_SATIS_H       += funnel.Y_IKA_SATIS_H;
                target.EY_BAYI_STOK        += funnel.EY_BAYI_STOK;
                target.YS_BAYI_STOK_H      += funnel.YS_BAYI_STOK_H;
                target.Y_RETAIL_SATIS_H    += funnel.Y_RETAIL_SATIS_H;
            }
            return target;
        }

    }

}