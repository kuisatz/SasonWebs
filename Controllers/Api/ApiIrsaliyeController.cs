using Antibiotic.Extensions;
using Microsoft.AspNetCore.Mvc;
using SasonBase.Sason.Models.ReportModels;
using SasonBase.Sason.Models.TableModels;
using SasonBase.Sason.Models.ViewModels;
using SasonWebs.Models.Api;
using SasonWebs.Models.Api.Entegrasyon.Muhasebe.Irsaliye.V1;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SasonWebs.Controllers.Api
{
    //http://localhost:59761/api/entegrasyon/muhasebe/irsaliye/debugirsaliyelistesi?serviceid=R540&servicepwd=esa4tr&startdate=20170101&finishdate=20171231

    [Route("api/entegrasyon/muhasebe/irsaliye")]
    [Produces("application/json")]
    public class ApiIrsaliyeController : Controller
    {
        [HttpGet]
        [HttpPost]
        [Route("IrsaliyeListesi")]
        public ApiMethodReturn<List<Irsaliye>> IrsaliyeListesi(string serviceId, string servicePwd, string startDate, string finishDate)
        {
            lock (ServisStaticValues.IrsaliyeLockObject)
            {
                ApiMethodReturn<List<Irsaliye>> ret = new ApiMethodReturn<List<Irsaliye>>();

                MethodReturn internalRet = new MethodReturn();

                DateTime date1 = new DateTime();
                DateTime date2 = new DateTime();

                bool isDate = true;
                if (isDate)
                    date1 = startDate.toDateTime("yyyyMMdd", out isDate);
                if (isDate)
                    date2 = finishDate.toDateTime("yyyyMMdd", out isDate);

                int maxProcessDayCount = 31;
#if DEBUG
                maxProcessDayCount = 366;
#endif

                if (ret.Ok && (date2 - date1).TotalDays > maxProcessDayCount)
                {
                    if (maxProcessDayCount != 1)
                        ret.Exception = $"Başlangıç ve Bitiş Arasında En Fazla {maxProcessDayCount} Gün Olabilir";
                    else
                        ret.Exception = "Başlangıç ve Bitiş Tarihi Aynı Gün Olmalı";
                }

                if (!isDate || ret.Ok == false)
                {
                    if (ret.Exception.isNullOrWhiteSpace())
                        ret.Exception = "Uyumsuz Tarih Biçimi";
                }
                else
                {
                    date2 = date2.endOfDay();
                    using (SasonWebAppPool appPool = SasonWebAppPool.Create)
                    {
                        SERVISLERWS servisWsInfo = SERVISLERWS.SelectServisWSInfo(serviceId, servicePwd, internalRet);
                        if (servisWsInfo.isNotNull())
                        {
                            servisWsInfo.BEKLEMESURESI = 3;
                            bool wsIzin = ServisStaticValues.IrsaliyeIzni(servisWsInfo);
                            ret.Info.Info1 = servisWsInfo.SERVISADI;
                            ret.Info.Info2 = servisWsInfo.SERVISID.ToString();
                            ret.Info.Info3 = servisWsInfo.WSKODU;
                            ret.Info.Info4 = $"{date1.ToString("dd.MM.yyyy HH.mm.ss")} - {date2.ToString("dd.MM.yyyy HH.mm.ss")}";

                            appPool.Parameters.SetParameter("ServiceId", servisWsInfo.SERVISID);

                            if (wsIzin)
                            {
                                ret.Data = R.Query<Irsaliye>(internalRet)
                                    .Where(
                                        t =>
                                            t.Tarih.Between(date1, date2)
                                            && t.SERVISID == servisWsInfo.SERVISID
                                            && t.SERVISSTOKISLEMID.In(1, 5)
                                            && (t.IrsaliyeNo.isNotNull() || t.IrsaliyeNo != "")
                                        //&& (t.FATURAID.isNull() || t.FATURAID == 0) //Daha Önce Çekildiği Düşünülerek Yalnız Faturalandırılmamış İrsaliye Kayıtları Gönderiliyor.
                                        )
                                    .ToList();
                                ret.Data = ret.Data.OrderByDescending(t => t.Tarih).toList();
                                ret.Info.Info5 = $"Data Count = {ret.Data.count()}";
                            }
                            else
                            {
                                ret.Exception = $@"İki İstek Arasındaki Zaman Farkı {servisWsInfo.BEKLEMESURESI} Dakikadan Az Olamaz. Bir Sonraki İstek Zamanınız {ServisStaticValues.GetServisInformation(servisWsInfo.SERVISID).SonFaturaWsIstekCevapZamani.AddMinutes((int)servisWsInfo.BEKLEMESURESI).ToString("dd.MM.yyyy HH:mm:ss")} veya Sonrası Olabilir";
                            }
                        }
                        else
                        {
                            if (internalRet.error())
                                ret.Exception = internalRet.ExceptionString;
                            else
                                ret.Exception = $"Servis Bulunamadı veya Hatalı Şifre";
                        }

                        ServisStaticValues.AddWsInfoToLog(servisWsInfo, "IrsaliyeListesi", ret.Ps, DateTime.Now, ret.Data.count(), ret.Exception, new List<string>().add(startDate).add(finishDate));
                    }
                }

                ret.Pf = DateTime.Now;
                return ret;
            }
        }














        //[HttpPost]
        //[Route("GetIrsaliye")]
        //public ApiMethodReturn<List<Irsaliye>> GetIrsaliye(string serviceId, string servicePwd, decimal irsaliyeId)
        //{
        //    ApiMethodReturn<List<Irsaliye>> ret = new ApiMethodReturn<List<Irsaliye>>();

        //    MethodReturn internalRet = new MethodReturn();

        //    using (SasonWebAppPool appPool = SasonWebAppPool.Create)
        //    {
        //        SERVISLERWS servisWsInfo = SERVISLERWS.SelectServisWSInfo(serviceId, servicePwd, internalRet);
        //        if (servisWsInfo.isNotNull())
        //        {
        //            appPool.Parameters.SetParameter("ServiceId", servisWsInfo.SERVISID);
        //            ret.Info.Info1 = servisWsInfo.SERVISADI;
        //            ret.Info.Info2 = servisWsInfo.SERVISID.ToString();
        //            ret.Info.Info3 = servisWsInfo.WSKODU;
        //            ret.Info.Info4 = $"{irsaliyeId}";

        //            ret.Data = R.Query<Irsaliye>(internalRet).where(t => t.SERVISID == servisWsInfo.SERVISID && t.ID == irsaliyeId).ToList();
        //            ret.Data = ret.Data.OrderByDescending(t => t.Tarih).toList();
        //        }
        //        else
        //        {
        //            if (internalRet.error())
        //                ret.Exception = internalRet.ExceptionString;
        //            else
        //                ret.Exception = $"Servis Bulunamadı veya Hatalı Şifre";
        //        }
        //    }

        //    ret.Pf = DateTime.Now;
        //    return ret;
        //}

        //[HttpGet]
        //[Route("DebugGetIrsaliyeIds")]
        //public ApiMethodReturn<List<decimal>> DebugGetIrsaliyeIds(string serviceId, string servicePwd, string startDate, string finishDate)
        //{
        //    return GetIrsaliyeIds(serviceId, servicePwd, startDate, finishDate);
        //}

        //[HttpPost]
        //[Route("GetIrsaliyeIds")]
        //public ApiMethodReturn<List<decimal>> GetIrsaliyeIds(string serviceId, string servicePwd, string startDate, string finishDate)
        //{
        //    ApiMethodReturn<List<decimal>> ret = new ApiMethodReturn<List<decimal>>();

        //    MethodReturn internalRet = new MethodReturn();

        //    DateTime date1 = new DateTime();
        //    DateTime date2 = new DateTime();

        //    bool isDate = true;
        //    if (isDate)
        //        date1 = startDate.toDateTime("yyyyMMdd", out isDate);
        //    if (isDate)
        //        date2 = finishDate.toDateTime("yyyyMMdd", out isDate);

        //    if (!isDate)
        //    {
        //        ret.Exception = "Uyumsuz Tarih Biçimi";
        //    }
        //    else
        //    {
        //        date2 = date2.endOfDay();
        //        using (SasonWebAppPool appPool = SasonWebAppPool.Create)
        //        {
        //            SERVISLERWS servisWsInfo = SERVISLERWS.SelectServisWSInfo(serviceId, servicePwd, internalRet);
        //            if (servisWsInfo.isNotNull())
        //            {
        //                ret.Info.Info1 = servisWsInfo.SERVISADI;
        //                ret.Info.Info2 = servisWsInfo.SERVISID.ToString();
        //                ret.Info.Info3 = servisWsInfo.WSKODU;
        //                ret.Info.Info4 = $"{date1.ToString("dd.MM.yyyy HH.mm.ss")} - {date2.ToString("dd.MM.yyyy HH.mm.ss")}";

        //                appPool.Parameters.SetParameter("ServiceId", servisWsInfo.SERVISID);

        //                    ret.Data = R.Query<IrsaliyeInfo000>(internalRet)
        //                        .Where(
        //                            t =>
        //                                t.Tarih.Between(date1, date2)
        //                                && t.SERVISID == servisWsInfo.SERVISID
        //                                && t.SERVISSTOKISLEMID.In(1, 5)
        //                                && (t.IrsaliyeNo.isNotNull() || t.IrsaliyeNo != "")
        //                                && (t.FATURAID.isNull() || t.FATURAID == 0)
        //                            )
        //                        .ToList().select(t=> t.ID).orderBy(t=> t).toList();
        //            }
        //            else
        //            {
        //                if (internalRet.error())
        //                    ret.Exception = internalRet.ExceptionString;
        //                else
        //                    ret.Exception = $"Servis Bulunamadı veya Hatalı Şifre";
        //            }
        //        }
        //    }

        //    ret.Pf = DateTime.Now;
        //    return ret;
        //}


    }
}