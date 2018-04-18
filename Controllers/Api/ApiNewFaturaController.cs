using Antibiotic.Extensions;
using Microsoft.AspNetCore.Mvc;
using SasonBase.Sason.Models.ReportModels;
using SasonBase.Sason.Models.TableModels;
using SasonWebs.Models.Api;
using SasonWebs.Models.Api.Entegrasyon.Muhasebe.Fatura.V1;
using SasonWebs.Models.Api.Entegrasyon.Muhasebe.Irsaliye.V1;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SasonWebs.Controllers.Api
{
    [Route("api")]
    [Produces("application/json")]
    public class ApiNewFaturaController : Controller
    {
        [HttpGet]
        [HttpPost]
        [Route("faturalartarih")]
        public ApiMethodReturn<List<Fatura>> Faturalar(string serviceId, string servicePwd, string startDate, string finishDate)
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

                        ServisStaticValues.AddWsInfoToLog(servisWsInfo, "faturalarTarih", ret.Ps, DateTime.Now, ret.Data.count(), ret.Exception,  new List<string>().add(startDate).add(finishDate));

                    }
                }

                ret.Pf = DateTime.Now;

                return ret;
            }
        }


        [HttpGet]
        [HttpPost]
        [Route("faturalaray")]
        public ApiMethodReturn<List<Fatura>> Faturalar(string serviceId, string servicePwd, int year, int month)
        {
            lock (ServisStaticValues.FaturaLockObject)
            {
                ApiMethodReturn<List<Fatura>> ret = new ApiMethodReturn<List<Fatura>>();
                MethodReturn internalRet = new MethodReturn();

                DateTime startDate = new DateTime(year, month, 1);
                DateTime finishDate = startDate.endOfMonth();

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
                        ret.Info.Info4 = $"{startDate.ToString("dd.MM.yyyy HH.mm.ss")} - {finishDate.ToString("dd.MM.yyyy HH.mm.ss")}";
                        if (faturaIzni)
                        {
                            ret.Data = R.Query<Fatura>(internalRet)
                                .Where(t =>
                                        t.SERVISID == servisWsInfo.SERVISID
                                        && t.FaturaTarihSaati.Between(startDate, finishDate)
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

                    ServisStaticValues.AddWsInfoToLog(servisWsInfo, "faturalaray", ret.Ps, DateTime.Now, ret.Data.count(), ret.Exception, new List<string>().add(year.toString()).add(month.toString()));
                }

                ret.Pf = DateTime.Now;
                return ret;
            }
        }


        [HttpGet]
        [HttpPost]
        [Route("faturalar")]
        public ApiMethodReturn<List<Fatura>> Faturalar(string serviceId, string servicePwd, decimal sonFaturaId)
        {
            lock (ServisStaticValues.FaturaLockObject)
            {
                ApiMethodReturn<List<Fatura>> ret = new ApiMethodReturn<List<Fatura>>();
                MethodReturn internalRet = new MethodReturn();

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
                        ret.Info.Info4 = $"FaturaId > {sonFaturaId}";
                        if (faturaIzni)
                        {
                            ret.Data = R.Query<Fatura>(internalRet)
                                .Where(t =>
                                        t.SERVISID == servisWsInfo.SERVISID
                                        && t.ID > sonFaturaId
                                        && t.FaturaNo.isNotNull())
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

                    ServisStaticValues.AddWsInfoToLog(servisWsInfo, "faturalar", ret.Ps, DateTime.Now, ret.Data.count(), ret.Exception, new List<string>().add(sonFaturaId.toString()));
                }

                ret.Pf = DateTime.Now;
                return ret;
            }
        }





        [HttpGet]
        [HttpPost]
        [Route("irsaliyeler")]
        public ApiMethodReturn<List<Irsaliye>> IrsaliyeListesi(string serviceId, string servicePwd, decimal sonirsaliyeid)
        {
            lock (ServisStaticValues.IrsaliyeLockObject)
            {
                ApiMethodReturn<List<Irsaliye>> ret = new ApiMethodReturn<List<Irsaliye>>();
                MethodReturn internalRet = new MethodReturn();

                using (SasonWebAppPool appPool = SasonWebAppPool.Create)
                {
                    SERVISLERWS servisWsInfo = SERVISLERWS.SelectServisWSInfo(serviceId, servicePwd, internalRet);
                    if (servisWsInfo.isNotNull())
                    {
                        servisWsInfo.BEKLEMESURESI = 0;
                        bool wsIzin = ServisStaticValues.IrsaliyeIzni(servisWsInfo);
                        ret.Info.Info1 = servisWsInfo.SERVISADI;
                        ret.Info.Info2 = servisWsInfo.SERVISID.ToString();
                        ret.Info.Info3 = servisWsInfo.WSKODU;
                        ret.Info.Info4 = $"IrsaliyeId > {sonirsaliyeid}";

                        appPool.Parameters.SetParameter("ServiceId", servisWsInfo.SERVISID);

                        if (wsIzin)
                        {
                            ret.Data = R.Query<Irsaliye>(internalRet)
                                .Where(
                                    t =>
                                        t.ID > sonirsaliyeid
                                        && t.SERVISID == servisWsInfo.SERVISID
                                        && t.SERVISSTOKISLEMID.In(1, 5)
                                        && (t.IrsaliyeNo.isNotNull() || t.IrsaliyeNo != "")
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
                    ServisStaticValues.AddWsInfoToLog(servisWsInfo, "IrsaliyeListesi", ret.Ps, DateTime.Now, ret.Data.count(), ret.Exception, new List<string>().add(sonirsaliyeid.ToString()));
                }

                ret.Pf = DateTime.Now;
                return ret;
            }
        }







        /// <summary>
        /// Özel Amaçlı Kullanım İçin Yapıldı (Servislerle Alakası Yok)
        /// </summary>
        /// <param name="faturaId"></param>
        /// <returns></returns>
        [HttpGet]
        [HttpPost]
        [Route("sfat")]
        public ApiMethodReturn<List<Fatura>> Fatura(decimal faturaId)
        {
            ApiMethodReturn<List<Fatura>> ret = new ApiMethodReturn<List<Fatura>>();
            MethodReturn internalRet = new MethodReturn();

            using (SasonWebAppPool appPool = SasonWebAppPool.Create)
            {
                decimal fServisId = appPool.EbaTestConnector.CreateQuery("SELECT SERVISID FROM FATURALAR WHERE ID = {ID}").Parameter("ID", faturaId).GetDataTable().FirstRowFirstColumnValue().cto<decimal>();
                SERVISLERWS servisWsInfo = R.Query<SERVISLERWS>().First(t => t.SERVISID == fServisId);
                if (servisWsInfo.isNotNull())
                {
                    appPool.Parameters.SetParameter("ServiceId", servisWsInfo.SERVISID);

                    ret.Info.Info1 = servisWsInfo.SERVISADI;
                    ret.Info.Info2 = servisWsInfo.SERVISID.ToString();
                    ret.Info.Info3 = servisWsInfo.WSKODU;
                    ret.Info.Info4 = $"FaturaId = {faturaId}";

                    ret.Data = R.Query<Fatura>(internalRet)
                        .Where(t => t.ID == faturaId)
                        .ToList();

                    FaturaTools.RepairFaturalar(ret.Data, servisWsInfo.SERVISID);

                    ret.Info.Info5 = $"Data Count = {ret.Data.count()}";
                    ret.Exception = internalRet.ExceptionString;
                }
                else
                {
                    if (internalRet.error())
                        ret.Exception = internalRet.ExceptionString;
                    else if (fServisId == 0)
                        ret.Info.Info5 = $"ID = {faturaId} Fatura Sistemde Kayıtlı Değil";
                    else
                        ret.Exception = $"Servis Bulunamadı veya Hatalı Şifre";
                }
            }

            ret.Pf = DateTime.Now;
            return ret;
        }



        /// <summary>
        /// Özel Amaçlı Kullanım İçin Yapıldı (Servislerle Alakası Yok)
        /// </summary>
        /// <param name="faturaId"></param>
        /// <returns></returns>
        [HttpGet]
        [HttpPost]
        [Route("sfat2")]
        public ApiMethodReturn<List<Fatura>> Fatura2(decimal servisId, decimal faturaTurId, string baslangic, string bitis)
        {
            ApiMethodReturn<List<Fatura>> ret = new ApiMethodReturn<List<Fatura>>();
            MethodReturn internalRet = new MethodReturn();

            DateTime date1 = new DateTime();
            DateTime date2 = new DateTime();

            bool isDate = true;
            if (isDate)
                date1 = baslangic.toDateTime("yyyyMMdd", out isDate);
            if (isDate)
                date2 = bitis.toDateTime("yyyyMMdd", out isDate);
            
            using (SasonWebAppPool appPool = SasonWebAppPool.Create)
            {
                //decimal fServisId = appPool.EbaTestConnector.CreateQuery("SELECT SERVISID FROM FATURALAR WHERE ID = {ID}").Parameter("ID", faturaId).GetDataTable().FirstRowFirstColumnValue().cto<decimal>();
                SERVISLERWS servisWsInfo = R.Query<SERVISLERWS>().First(t => t.SERVISID == servisId);
                if (servisWsInfo.isNotNull())
                {
                    appPool.Parameters.SetParameter("ServiceId", servisWsInfo.SERVISID);

                    ret.Info.Info1 = servisWsInfo.SERVISADI;
                    ret.Info.Info2 = servisWsInfo.SERVISID.ToString();
                    ret.Info.Info3 = servisWsInfo.WSKODU;
                    //ret.Info.Info4 = $"FaturaId = {faturaId}";

                    if (isDate)
                    {
                        ret.Data = R.Query<Fatura>(internalRet)
                            .Where(t => t.SERVISID == servisId && t.FATURATURID == faturaTurId && t.FaturaTarihSaati.Between(date1, date2))
                            .ToList();
                    }
                    else
                    {
                        ret.Data = R.Query<Fatura>(internalRet)
                            .Where(t => t.SERVISID == servisId && t.FATURATURID == faturaTurId)
                            .ToList();
                    }

                    FaturaTools.RepairFaturalar(ret.Data, servisWsInfo.SERVISID);

                    ret.Info.Info5 = $"Data Count = {ret.Data.count()}";
                    ret.Exception = internalRet.ExceptionString;
                }
                else
                {
                    if (internalRet.error())
                        ret.Exception = internalRet.ExceptionString;
                    else
                        ret.Exception = $"Servis Bulunamadı veya Hatalı Şifre";
                }
            }

            ret.Pf = DateTime.Now;
            return ret;
        }

    }
}