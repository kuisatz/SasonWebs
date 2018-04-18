using Antibiotic.Extensions;
using Microsoft.AspNetCore.Mvc;
using SasonBase.Sason.Models.ReportModels;
using SasonBase.Sason.Models.TableModels;
using SasonBase.Sason.Models.ViewModels;
using SasonWebs.Models.Api;
using SasonWebs.Models.Api.Entegrasyon.Muhasebe.Fatura.V1;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Configuration;

namespace SasonWebs.Controllers.Api
{
    [Route("api/entegrasyon/muhasebe/fatura")]
    [Produces("application/json")]
    public class ApiFaturaController : Controller
    {
        [HttpGet][HttpPost]
        [Route("KesilenFaturaListesi")]
        public ApiMethodReturn<List<Fatura>> KesilenFaturaListesi(string serviceId, string servicePwd, string startDate, string finishDate)
        {
            lock (ServisStaticValues.FaturaLockObject)
            {
                ApiMethodReturn<List<Fatura>> ret = new ApiMethodReturn<List<Fatura>>();
                MethodReturn internalRet = new MethodReturn();

                DateTime date1 = new DateTime();
                DateTime date2 = new DateTime();

                bool isDate = true;
                if (isDate)
                    date1 = startDate.toDateTime("yyyyMMdd", out isDate);
                if (isDate)
                    date2 = finishDate.toDateTime("yyyyMMdd", out isDate);

                int maxProcessDayCount = 30;
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
                            bool faturaIzni = ServisStaticValues.FaturaIzni(servisWsInfo);
                            appPool.Parameters.SetParameter("ServiceId", servisWsInfo.SERVISID);
                            ret.Info.Info1 = servisWsInfo.SERVISADI;
                            ret.Info.Info2 = servisWsInfo.SERVISID.ToString();
                            ret.Info.Info3 = servisWsInfo.WSKODU;
                            ret.Info.Info4 = $"{date1.ToString("dd.MM.yyyy HH.mm.ss")} - {date2.ToString("dd.MM.yyyy HH.mm.ss")}";
                            if (faturaIzni)
                            {
                                ret.Data = R.Query<Fatura>(internalRet)
                                    .Where(t =>
                                            t.SERVISID == servisWsInfo.SERVISID
                                            && t.FaturaTarihSaati.Between(date1, date2)
                                            && t.FaturaNo.isNotNull()
                                        )
                                    .OrderByDescending(t => t.ID)
                                    .ToList();

                                FaturaTools.RepairFaturalar(ret.Data, servisWsInfo.SERVISID);
                                ret.Info.Info5 = $"Data Count = {ret.Data.count()}";
                                ret.Exception = internalRet.ExceptionString;
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
                        ServisStaticValues.AddWsInfoToLog(servisWsInfo, "kesilenfaturalistesi", ret.Ps, DateTime.Now, ret.Data.count(), ret.Exception, new List<string>().add(startDate).add(finishDate));
                    }
                }

                ret.Pf = DateTime.Now;
                return ret;
            }
        }




        //[HttpGet]
        //[Route("DebugGetFatura")]
        //public ApiMethodReturn<List<Fatura>> DebugGetFatura(string serviceId, string servicePwd, decimal faturaId)
        //{
        //    return GetFatura(serviceId, servicePwd, faturaId);
        //}

        //[HttpPost]
        //[Route("GetFatura")]
        //public ApiMethodReturn<List<Fatura>> GetFatura(string serviceId, string servicePwd, decimal faturaId)
        //{
        //    ApiMethodReturn<List<Fatura>> ret = new ApiMethodReturn<List<Fatura>>();
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
        //            ret.Info.Info4 = faturaId.ToString();

        //            ret.Data = R.Query<Fatura>(internalRet).Where(t => t.SERVISID == servisWsInfo.SERVISID && t.ID == faturaId).ToList();

        //            List<string> isEmirNolar = ret.Data.where(t => t.IsEmirNo.isNotNullOrWhiteSpace()).select(t => t.IsEmirNo).toList().Distinct().toList();
        //            List<ReportData_VT_ISEMIRDETAYLARFATURA> isEmirDetaylarFatura = R.Query<ReportData_VT_ISEMIRDETAYLARFATURA>().Where(t => t.SERVISID == servisWsInfo.SERVISID && t.ISEMIRNO.In(isEmirNolar)).ToList();

        //            ret.Data.forEach(fatura =>
        //            {
        //                fatura.IsEmirDetaylarFatura = isEmirDetaylarFatura.where(t => t.ISEMIRNO == fatura.IsEmirNo).toList();
        //            });


        //            ret.Data.forEach(fatura =>
        //            {
        //                fatura.FaturaDetaylar.forEach(detay =>
        //                {
        //                    ReportData_VT_ISEMIRDETAYLARFATURA foundKodDef = fatura.IsEmirDetaylarFatura.first(t => t.KOD == detay.Kod || t.ORJINALKOD == detay.Kod);
        //                    if (foundKodDef.isNotNull())
        //                    {
        //                        if (detay.Kod == foundKodDef.KOD)
        //                            detay.Kod2 = foundKodDef.ORJINALKOD;
        //                        if (detay.Kod == foundKodDef.ORJINALKOD)
        //                            detay.Kod2 = foundKodDef.KOD;
        //                    }

        //                    if (detay.Kod2.isNullOrWhiteSpace())
        //                        detay.Kod2 = detay.Kod;
        //                });
        //            });


        //            ret.Exception = internalRet.ExceptionString;
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
        //[Route("DebugGetFaturaIds")]
        //public ApiMethodReturn<List<decimal>> DebugGetFaturaIds(string serviceId, string servicePwd, string startDate, string finishDate)
        //{
        //    return GetFaturaIds(serviceId, servicePwd, startDate, finishDate);
        //}

        //[HttpPost]
        //[Route("GetFaturaIds")]
        //public ApiMethodReturn<List<decimal>> GetFaturaIds(string serviceId, string servicePwd, string startDate, string finishDate)
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
        //        ret.Exception = "Uyumsuz Tarih Biçimi";
        //    else
        //    {
        //        date2 = date2.endOfDay();
        //        using (SasonWebAppPool appPool = SasonWebAppPool.Create)
        //        {
        //            SERVISLERWS servisWsInfo = SERVISLERWS.SelectServisWSInfo(serviceId, servicePwd, internalRet);
        //            if (servisWsInfo.isNotNull())
        //            {
        //                appPool.Parameters.SetParameter("ServiceId", servisWsInfo.SERVISID);

        //                ret.Info.Info1 = servisWsInfo.SERVISADI;
        //                ret.Info.Info2 = servisWsInfo.SERVISID.ToString();
        //                ret.Info.Info3 = servisWsInfo.WSKODU;
        //                ret.Info.Info4 = $"{date1.ToString("dd.MM.yyyy HH.mm.ss")} - {date2.ToString("dd.MM.yyyy HH.mm.ss")}";

        //                ret.Data = R.Query<FaturaInfo000>(internalRet)
        //                    .Where(t =>
        //                            t.SERVISID == servisWsInfo.SERVISID
        //                            && t.FaturaTarihSaati.Between(date1, date2)
        //                            && t.FaturaNo.isNotNull()
        //                        )
        //                    .ToList().select(t=> t.ID).orderBy(t=> t).toList();

        //                ret.Exception = internalRet.ExceptionString;
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



        ////GET: api/fatura/entegrasyon/muhasebe/kesilenfaturalistesi
        //[HttpGet]
        //[Route("DebugKesilenFatura")]
        //public ApiMethodReturn<List<Fatura>> DebugKesilenFatura(int serviceId, string servicePwd, int faturaId)
        //{
        //   ApiMethodReturn<List<Fatura>> ret = new ApiMethodReturn<List<Fatura>>();
        //    ret.Data = new List<Fatura>();
        //    using (SasonAppPool appPool = SasonAppPool.Create)
        //    {
        //        VT_SERVISLER servis = VT_SERVISLER.SelectItem(serviceId);
        //        if (servis.isNotNull() && servis.PASSWORD == servicePwd)
        //            ret.Data = R.Query<Fatura>().Where(t => t.ID == faturaId).OrderBy(t => t.ID).ToList();
        //        else
        //            ret.Exception = $"Servis Bulunamadı veya Servis Şifresi Doğru Değil";
        //    }
        //    return ret;
        //}

        /*
         * 
         *            
            Kullanimı
        
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            var response = await client.PostAsJsonAsync("http://localhost:59761/api/invoice/getinvoices", new xxxx() { ServiceId = 5, ServicePwd="esa4tr" });
            response.EnsureSuccessStatusCode();
         * 
        public class ParamServisInfo
        {
            public int      ServiceId   { get; set; }
            public string   ServicePwd  { get; set; }
        }

        //POST: api/invoice/GetInvoices
        [HttpPost]
        [Route("GetInvoices")]
        public ApiMethodReturn<List<Invoice>> GetInvoices([FromBody] ParamServisInfo servisInfo)//  int serviceId, string servicePwd)
        {
            ApiMethodReturn<List<Invoice>> ret = new ApiMethodReturn<List<Invoice>>();
            ret.Data = new List<Invoice>();
            using (SasonAppPool appPool = SasonAppPool.Create)
            {
                VT_SERVISLER servis = VT_SERVISLER.SelectItem(servisInfo.ServiceId);
                if (servis.isNotNull() && servis.PASSWORD == servisInfo.ServicePwd)
                    ret.Data = R.Query<Invoice>().ToList();
                else
                    ret.Exception = $"Servis Bulunamadı veya Servis Şifresi Doğru Değil";
            }
            return ret;
        }
        */

    }
}



/*
 * 
//List<string> isEmirNolar = ret.Data.where(t => t.IsEmirNo.isNotNullOrWhiteSpace()).select(t => t.IsEmirNo).toList().Distinct().toList();

                        //List<ReportData_VT_ISEMIRDETAYLARFATURA> isEmirDetaylarFatura = null;
                        //if (isEmirNolar.isNotEmpty())
                        //    isEmirDetaylarFatura = R.Query<ReportData_VT_ISEMIRDETAYLARFATURA>().Where(t => t.SERVISID == servisWsInfo.SERVISID && t.ISEMIRNO.In(isEmirNolar)).ToList();

                        //ret.Data.forEach(fatura =>
                        //{
                        //    fatura.IsEmirDetaylarFatura = isEmirDetaylarFatura.where(t => t.ISEMIRNO == fatura.IsEmirNo).toList();
                        //});

                        //ret.Data.forEach(fatura =>
                        //{
                        //    fatura.FaturaDetaylar.forEach(detay =>
                        //    {
                        //        ReportData_VT_ISEMIRDETAYLARFATURA foundKodDef = fatura.IsEmirDetaylarFatura.first(t => t.KOD == detay.Kod || t.ORJINALKOD == detay.Kod);
                        //        if (foundKodDef.isNotNull())
                        //        {
                        //            if (detay.Kod == foundKodDef.KOD)
                        //                detay.Kod2 = foundKodDef.ORJINALKOD;
                        //            if (detay.Kod == foundKodDef.ORJINALKOD)
                        //                detay.Kod2 = foundKodDef.KOD;
                        //        }

                        //        if (detay.Kod2.isNullOrWhiteSpace())
                        //            detay.Kod2 = detay.Kod;
                        //    });
                        //}); * 
 * */
