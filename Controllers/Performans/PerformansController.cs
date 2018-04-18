using Antibiotic.Database.Field;
using Antibiotic.Database.Row;
using Antibiotic.Extensions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SasonWebs.Controllers.Performans.Models;
using SasonWebs.Controllers.Performans.Models.Crm.Merkez;
using SasonWebs.Controllers.Performans.Models.Crm.Bayi;
using SasonWebs.Controllers.Performans.Models.Sason;
using SasonWebs.Models.Api;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace SasonWebs.Controllers.Performans
{
    [Route("api/performans")]
    [Produces("application/json")]
    public partial class PerformansController : Helpers.BaseController
    {
        private void CheckDebugUser()
        {
            if ("DEBUG".ToUser().isNull())
            {
                Login("ilyas.sen", "isgp2017", "tr");
            }
        }

        #region Login
        [Route("login"), HttpGet, HttpPost]
        [ResponseCache(NoStore = true, Duration = 0)]
        public ApiMethodReturn<ResponseUserInfo> Login(string username, string password, string language)
        {
            ApiMethodReturn<ResponseUserInfo> ret = new ApiMethodReturn<ResponseUserInfo>();
            lock (Statics.UserLockObject)
            {
                username = username.toString();
                password = password.toString();
                language = language.toString();

                MethodReturn mr = new MethodReturn();
                ServerUserInfo serverUserInfo = null;

//#if DEBUG
//                username = "tuncay.bekiroglu";
//                password = "tb2017";
//#endif

                using (SasonWebAppPool appPool = SasonWebAppPool.Create)
                {
                    DataTable dtb = appPool.EbaTestConnector.CreateQuery(@"
                        SELECT * FROM SASON.MOBILUSER WHERE 
                                lower(USERNAME) = {username}
                                and lower(PASSWORD) = {password} 
 
                        ")
                        .Parameter("username", username.ToLower())
                        .Parameter("password", password.ToLower())
                        .GetDataTable(mr);

                    if (mr.ok())
                    {
                        if (dtb.IsEmpty())
                        {
                            ret.Data.Token = "TOKEN:EMPTY";
                            ret.ExceptionCode = "USER_NOTFOUND";
                        }
                        else
                        {
                            DataRow dtr = dtb.FirstRow();

                            List<decimal> servisIdler = dtr["SERVISIDLER"].toString().split().select(t => t.cto<decimal>()).toList();

                            if (dtr["SERVISIDLER"].toString().isNullOrWhiteSpace())
                                ret.ExceptionCode = "USER_EMPTY_SERVICES";

                            if (ret.Ok)
                            {
                                ret.Data.ServisName = dtr["SERVISNAME"].toString();
                                ret.Data.UserName   = dtr["USERNAME"].toString();
                                ret.Data.Name       = dtr["NAME"].toString();
                                ret.Data.Language   = language;
                                ret.Data.Servisler  = Statics.GetResponseServisler(servisIdler);
                                ret.Data.UserImage  = dtr["USERIMAGE"].toString();
                                //ret.Data.Token = HttpContext.Connection.RemoteIpAddress.toString().Replace(".", "");
#if DEBUG
                                ret.Data.Token = "DEBUG";
#endif

                                serverUserInfo = new ServerUserInfo()
                                {
                                    Token        = ret.Data.Token,
                                    ServisName   = ret.Data.ServisName,
                                    UserName     = ret.Data.UserName,
                                    Name         = ret.Data.Name,
                                    Language     = ret.Data.Language,
                                    UserImage    = ret.Data.UserImage,
                                    UserId       = dtr["ID"].cto<decimal>(),
                                    BayiIdler    = dtr["BAYIID"].toString().split().select(t=> t.cto<decimal>()).toList(),
                                    ServisIdler  = Statics.GetServerServisler(servisIdler).select(t=> t.ServisId).toList(),
                                    ClientIp     = HttpContext.Connection.RemoteIpAddress.toString(),
                                };
                            }
                        }
                    }
                    else
                        ret.Exception = mr.ExceptionString;
                }

                if (serverUserInfo.isNotNull())
                {
                    Statics.SetServerUser(serverUserInfo);
                }
                else
                {
                    serverUserInfo = new ServerUserInfo()
                    {
                        UserName = username,
                        Password = password,
                        Language = language,
                        ClientIp = HttpContext.Connection.RemoteIpAddress.toString(),
                    };
                    Statics.AppendToErrLog(serverUserInfo, ret.ExceptionCode, ret.Exception);
                }
                ret.Pf = DateTime.Now;
            }
            return ret;
        }
        #endregion

        #region GetCardDetails
        [HttpGet, HttpPost, Route("getCardDetails")]
        [ResponseCache(NoStore = true, Duration = 0)]
        public object GetCardDetails(string token, string id, string timeType, string servisIds)
        {

#if DEBUG
            CheckDebugUser();
#endif

            object ret = null;

            //List<decimal> targetServisIds = null;
            ServerUserInfo userInfo = token.ToUser();

            eTimes eTimeType = timeType.ToEnum<eTimes>();

            if (userInfo.isNotNull())
            {
                using (var ap = SasonWebAppPool.CreateMask)
                {
                    //targetServisIds = servisIds.ToServisIds(userInfo.ServisIdler);
                    switch (id)
                    {
                        case "ARAC_GIRIS":
                        case "ISEMIR_ACILAN":
                        case "ISEMIR_KAPANAN":
                            ret = Models.Sason.Sason_AracGirisSayilari.GetCardDetails(eTimeType, userInfo.ServisIdler);
                            break;
                        case "ALIS_FATURALARI":
                        case "ISEMRI_FATURALARI":
                        case "SATIS_FATURALARI":
                        case "ICMAL_FATURALARI":
                        case "DISHIZMET_FATURALARI":
                            ret = Models.Sason.Sason_ServislerFatura.GetCardDetails(eTimeType, userInfo.ServisIdler);
                            break;
                        case "YEDEK_PARCA":
                            ret = Models.Sason.Sason_YedekParca.Select_YedekParca_GraphData(eTimeType, userInfo.ServisIdler);
                            break;
                        case "BAYI_STOKLAR":
                            ret = Models.Crm.Bayi.Bayi_Stok.GetBayiStokGraphData(userInfo.BayiIdler);
                            break;
                        case "MAN_STOK_TOPLAM_FATURALANAN":
                            ret = Man_AracFatura.Select_AylikFaturalananAracSayilari(userInfo.BayiIdler);
                            //ret = ManStok.Select_ManStokData(userInfo.BayiIdler);
                            break;
                        case "MAN_TOPLAM_STOK":
                            ret = Man_Stok.Select_ManStokData(userInfo.BayiIdler);
                            break;
                        case "BAYI_TOPLAM_FATURALANAN":
                            ret = Models.Crm.Bayi.Bayi_FaturaSayilar.Select_AylikFaturalananAracSayilari(userInfo.BayiIdler);
                            break;
                        case "BAYI_FIRSATLAR":
                            ret = Models.Crm.Bayi.Bayi_Firsatlar.GetCardDetails(eTimeType, userInfo.BayiIdler);
                            break;
                        case "BAYI_AKTIVITELER":
                            ret = Models.Crm.Bayi.Bayi_Aktivite.GetCardDetails(eTimeType, userInfo.BayiIdler);
                            break;
                        case "CIROLAR":
                            ret = Models.Sason.Sason_ServislerCiro.GetCirolarDetay(eTimeType, userInfo.ServisIdler);
                            break;
                        case "ACIK_ISEMIRLERI":
                            ret = Models.Sason.Sason_AcikEmirleri.GetAcikIsEmirleri(userInfo);
                            break;
                    }
                }
            }
            else
            {
                ret = new ApiMethodReturn<string>() { Exception = "TOKEN:NOT_FOUND" };
                Statics.AppendToErrLogTokenNotFound(token);
            }
            return ret;
        }
        #endregion

        [HttpGet, HttpPost, Route("getperformansdata")]
        [ResponseCache(NoStore = true, Duration = 0)]
        public object GetPerformansData(string token)
        {
#if DEBUG
            CheckDebugUser();
            token = "DEBUG";
#endif
            ApiMethodReturn<List<Sason_TeknisyenPerformans_GraphData>> ret = new ApiMethodReturn<List<Sason_TeknisyenPerformans_GraphData>>();
            ret = Sason_TeknisyenPerformans.GetTeknisyenGraphData();

            //ServerUserInfo userInfo = token.ToUser();
            //if (userInfo.isNotNull())
            //{
            //    //eTimes eTimeType = timeType.ToEnum<eTimes>();
            //    ret = Bayi_FaturaAdlar.GetBayiFaturalarGraphData(userInfo.BayiIdler);
            //}
            //else
            //{
            //    ret.Exception = "TOKEN:NOT_FOUND";
            //    Statics.AppendToErrLogTokenNotFound(token);
            //}
            ret.Pf = DateTime.Now;
            return ret;
        }



        [HttpGet, HttpPost, Route("getbayifaturalarad")]
        [ResponseCache(NoStore = true, Duration = 0)]
        public object GetBayiFaturalarAd(string token)
        {
#if DEBUG
            CheckDebugUser();
            token = "DEBUG";
#endif
            ApiMethodReturn<List<Bayi_FaturaAdlar>> ret = new ApiMethodReturn<List<Bayi_FaturaAdlar>>();
            ServerUserInfo userInfo = token.ToUser();
            if (userInfo.isNotNull())
            {
                //eTimes eTimeType = timeType.ToEnum<eTimes>();
                ret = Bayi_FaturaAdlar.GetBayiFaturalarGraphData(userInfo.BayiIdler);
            }
            else
            {
                ret.Exception = "TOKEN:NOT_FOUND";
                Statics.AppendToErrLogTokenNotFound(token);
            }
            ret.Pf = DateTime.Now;
            return ret;
        }


        [HttpGet, HttpPost, Route("getmanprojeler")]
        [ResponseCache(NoStore = true, Duration = 0)]
        public object GetManProjeler(string token)
        {
#if DEBUG
            CheckDebugUser();
            token = "DEBUG";
#endif
            ApiMethodReturn<List<Man_Projeler_GridData>> ret = new ApiMethodReturn<List<Man_Projeler_GridData>>();
            ServerUserInfo userInfo = token.ToUser();
            if (userInfo.isNotNull())
            {
                //eTimes eTimeType = timeType.ToEnum<eTimes>();
                ret = Man_Projeler.GetManProjelerGridData(userInfo.BayiIdler);
            }
            else
            {
                ret.Exception = "TOKEN:NOT_FOUND";
                Statics.AppendToErrLogTokenNotFound(token);
            }
            ret.Pf = DateTime.Now;
            return ret;
        }

        [HttpGet, HttpPost, Route("getmanprojelergraph")]
        [ResponseCache(NoStore = true, Duration = 0)]
        public object GetManProjelerGraph(string token)
        {
#if DEBUG
            CheckDebugUser();
            token = "DEBUG";
#endif
            ApiMethodReturn<List<Man_Projeler_GraphData>> ret = new ApiMethodReturn<List<Man_Projeler_GraphData>>();
            ServerUserInfo userInfo = token.ToUser();
            if (userInfo.isNotNull())
            {
                //eTimes eTimeType = timeType.ToEnum<eTimes>();
                ret = Man_Projeler.GetManProjelerGraphData(userInfo.BayiIdler);
            }
            else
            {
                ret.Exception = "TOKEN:NOT_FOUND";
                Statics.AppendToErrLogTokenNotFound(token);
            }
            ret.Pf = DateTime.Now;
            return ret;
        }




        [HttpGet, HttpPost, Route("getsalesfunnel")]
        [ResponseCache(NoStore = true, Duration = 0)]
        public ApiMethodReturn<List<Man_SalesFunnel_GraphData>> GetSalesFunnel(string token, string timeType)
        {
            ApiMethodReturn<List<Man_SalesFunnel_GraphData>> ret = new ApiMethodReturn<List<Man_SalesFunnel_GraphData>>();
            ServerUserInfo userInfo = token.ToUser();
            if (userInfo.isNotNull())
            {
                eTimes eTimeType = timeType.ToEnum<eTimes>();
                ret = Man_SalesFunnel.Select_SalesFunnel_WsData(eTimeType, userInfo.BayiIdler);
            }
            else
            {
                ret.Exception = "TOKEN:NOT_FOUND";
                Statics.AppendToErrLogTokenNotFound(token);
            }
            ret.Pf = DateTime.Now;
            return ret;
        }

        [HttpGet, HttpPost, Route("getsalesfunnelkritik")]
        [ResponseCache(NoStore = true, Duration = 0)]
        public ApiMethodReturn<List<Man_SalesFunnelKritik_GraphData>> GetSalesFunnelKritik(string token)
        {
            ApiMethodReturn<List<Man_SalesFunnelKritik_GraphData>> ret = new ApiMethodReturn<List<Man_SalesFunnelKritik_GraphData>>();
            ServerUserInfo userInfo = token.ToUser();
            if (userInfo.isNotNull())
                ret = Man_SalesFunnelKritik.Select_SalesFunnel_Kritik_WsData(userInfo.BayiIdler);
            else
            {
                ret.Exception = "TOKEN:NOT_FOUND";
                Statics.AppendToErrLogTokenNotFound(token);
            }
            ret.Pf = DateTime.Now;
            return ret;
        }

        [HttpGet,HttpPost, Route("getyedekparca")]
        [ResponseCache(NoStore = true, Duration = 0)]
        public ApiMethodReturn<SasonYedekParcaWsDataSource> GetYedekParca(string token, string timeType)
        {
            ApiMethodReturn<SasonYedekParcaWsDataSource> ret = new ApiMethodReturn<SasonYedekParcaWsDataSource>();
            ServerUserInfo userInfo = token.ToUser();
            if (userInfo.isNotNull())
            {
                ret = Sason_YedekParca.Select_YedekParca_GraphData(timeType.ToEnum<eTimes>(), userInfo.ServisIdler);
            }
            else
            {
                ret.Exception = "TOKEN:NOT_FOUND";
                Statics.AppendToErrLogTokenNotFound(token);
            }
            ret.Pf = DateTime.Now;
            return ret;
        }

        [HttpGet, HttpPost, Route("getkamyonsatisdurumu")]
        [ResponseCache(NoStore = true, Duration = 0)]
        public object getkamyonsatisdurumu(string token)
        {
            ApiMethodReturn<List<Man_KamyonSatisDurumu_GridData>> ret = new ApiMethodReturn<List<Man_KamyonSatisDurumu_GridData>>();
            ServerUserInfo userInfo = token.ToUser();
            if (userInfo.isNotNull())
            {
                ret = Man_KamyonSatisDurumu.GetSatisDurumlari(userInfo.BayiIdler);
            }
            else
            {
                ret.Exception = "TOKEN:NOT_FOUND";
                Statics.AppendToErrLogTokenNotFound(token);
            }
            ret.Pf = DateTime.Now;
            return ret;
        }


        [HttpGet, HttpPost, Route("getureticilersatissayi")]
        [ResponseCache(NoStore = true, Duration = 0)]
        public object getureticilersatissayi(string token)
        {
#if DEBUG
            CheckDebugUser();
            token = "DEBUG";
#endif
            ApiMethodReturn<List<Ureticiler_Satis_GraphData>> ret = new ApiMethodReturn<List<Ureticiler_Satis_GraphData>>();
            ServerUserInfo userInfo = token.ToUser();
            if (userInfo.isNotNull())
            {
                ret = Man_Ureticiler_Satis.GetUreticilerGraphData(userInfo.BayiIdler);
            }
            else
            {
                ret.Exception = "TOKEN:NOT_FOUND";
                Statics.AppendToErrLogTokenNotFound(token);
            }
            ret.Pf = DateTime.Now;
            return ret;
        }

        [HttpGet, HttpPost, Route("getureticilersatissayibar")]
        [ResponseCache(NoStore = true, Duration = 0)]
        public object getureticilersatissayibar(string token)
        {
#if DEBUG
            CheckDebugUser();
            token = "DEBUG";
#endif
            ApiMethodReturn<List<Ureticiler_Satis_GraphData>> ret = new ApiMethodReturn<List<Ureticiler_Satis_GraphData>>();
            ServerUserInfo userInfo = token.ToUser();
            if (userInfo.isNotNull())
            {
                ret = Man_Ureticiler_Satis.GetUreticilerBarGraphData(userInfo.BayiIdler);
            }
            else
            {
                ret.Exception = "TOKEN:NOT_FOUND";
                Statics.AppendToErrLogTokenNotFound(token);
            }
            ret.Pf = DateTime.Now;
            return ret;
        }

        [HttpGet, HttpPost, Route("getmansatissayibar")]
        [ResponseCache(NoStore = true, Duration = 0)]
        public object getmansatissayibar(string token)
        {
#if DEBUG
            CheckDebugUser();
            token = "DEBUG";
#endif
            ApiMethodReturn<List<Ureticiler_Satis_GraphData>> ret = new ApiMethodReturn<List<Ureticiler_Satis_GraphData>>();
            ServerUserInfo userInfo = token.ToUser();
            if (userInfo.isNotNull())
            {
                ret = Man_Ureticiler_Satis.GetManSatisSayiBar(userInfo.BayiIdler);
            }
            else
            {
                ret.Exception = "TOKEN:NOT_FOUND";
                Statics.AppendToErrLogTokenNotFound(token);
            }
            ret.Pf = DateTime.Now;
            return ret;
        }

        #region GetMusteriMemnuniyet
        [HttpGet, HttpPost, Route("getMemnuniyet")]
        [ResponseCache(NoStore = true, Duration = 0)]
        public ApiMethodReturn<List<MemnuniyetResult>> GetMusteriMemnuniyet(string token, string servisIds)
        {
            ApiMethodReturn<List<MemnuniyetResult>> ret = new ApiMethodReturn<List<MemnuniyetResult>>();
            ServerUserInfo userInfo = token.ToUser();
            if (userInfo.isNotNull())
                ret = Man_Memnuniyet.GetMemnuniyetResults(servisIds.ToServisIds(userInfo.ServisIdler));
            else
            {
                ret.Exception = "TOKEN:NOT_FOUND";
                Statics.AppendToErrLogTokenNotFound(token);
            }
            ret.Pf = DateTime.Now;
            return ret;
        }
        #endregion





        //Func<object> t1 = delegate ()
        //{
        //    ApiMethodReturn<MobilAgsData> subret = new ApiMethodReturn<MobilAgsData>();
        //    switch (id)
        //    {
        //        case "ARAC_GIRIS":
        //        case "ISEMIR_ACILAN":
        //        case "ISEMIR_KAPANAN":
        //            subret = MobilAGS.GetCardDetails(eTimeType, targetServisIds);
        //            break;
        //    }
        //    subret.Pf = DateTime.Now;
        //    return subret;
        //};

        //Func<object> t2 = delegate ()
        //{
        //    ApiMethodReturn<MobilFaturalarData> subret = new ApiMethodReturn<MobilFaturalarData>();
        //    switch (id)
        //    {
        //        case "ALIS_FATURALARI":
        //        case "ISEMRI_FATURALARI":
        //        case "SATIS_FATURALARI":
        //        case "ICMAL_FATURALARI":
        //        case "DISHIZMET_FATURALARI":
        //            subret = MobilFaturalar.GetCardDetails(eTimeType, targetServisIds);
        //            break;
        //    }
        //    subret.Pf = DateTime.Now;
        //    return subret;
        //};

        //[HttpGet, HttpPost, Route("getisemirler")]
        //public ApiMethodReturn<List<MobilAGS>> GetIsEmirler(string token, string startDate, string finishDate, string servisIds)
        //{
        //    ApiMethodReturn<List<MobilAGS>> ret = new ApiMethodReturn<List<MobilAGS>>();

        //    ServerUserInfo userInfo = token.ToUser();
        //    if (userInfo.isNotNull())
        //    { 
        //        MethodReturn mr = MobilAGS.Select_IsEmirleri(startDate.ToParamDate(), finishDate.ToParamDate(), servisIds.ToServisIds(userInfo.ServisIdler));
        //        if (mr.ok())
        //            ret.Data = mr.Result as List<MobilAGS>;
        //        else
        //            ret.Exception = mr.ExceptionString;
        //    }

        //    ret.Pf = DateTime.Now;
        //    return ret;
        //}

        //[HttpGet, HttpPost, Route("getisemirleraylik")]
        //public ApiMethodReturn<List<MobilAGS>> GetIsEmirlerAylik(string token, string startDate, string finishDate, string servisIds)
        //{
        //    ApiMethodReturn<List<MobilAGS>> ret = new ApiMethodReturn<List<MobilAGS>>();

        //    ServerUserInfo userInfo = token.ToUser();
        //    if (userInfo.isNotNull())
        //    {
        //        MethodReturn mr = MobilAGS.Select_IsEmirleri_Aylik_Toplam(startDate.ToParamDate(), finishDate.ToParamDate(), servisIds.ToServisIds(userInfo.ServisIdler));
        //        if (mr.ok())
        //            ret.Data = mr.Result as List<MobilAGS>;
        //        else
        //            ret.Exception = mr.ExceptionString;
        //    }

        //    ret.Pf = DateTime.Now;
        //    return ret;
        //}


        //[HttpPost,HttpGet,Route("getaracgiris")]
        //public ApiMethodReturn<List<AracGiris>> GetAracGiris(string token, string startDate, string finishDate, IEnumerable<decimal> serviceids)
        //{
        //    MethodReturn mr = new MethodReturn();

        //    ServerUserInfo serverUserInfo = Statics.GetServerUserInfo(token);
        //    if (serverUserInfo.isNull())
        //    {

        //    }

        //    ApiMethodReturn<List<AracGiris>> ret = new ApiMethodReturn<List<AracGiris>>();

        //    DateTime startDate = DateTime.Now.AddDays(-1).startOfMonth();
        //    DateTime finishDate = DateTime.Now.endOfDay();

        //    using (SasonWebAppPool appPool = SasonWebAppPool.Create)
        //    {
        //        ret.Data = appPool.EbaTestConnector.CreateQuery(@"
        //            SELECT 
        //                    AGS ARACGIRISSAYISI, IES ISEMRISAYISI, ISLS ISLEMSAYISI, TARIH
        //            FROM 
        //                    RP_AGS 
        //            WHERE 
        //                    HASHSERVISID = {hashservisid}
        //                    AND TARIH BETWEEN {startdate} and {finishdate}
        //            ORDER BY TARIH
        //            ")
        //            .Parameter("hashservisid", hashservisid)
        //            .Parameter("startdate", startDate)
        //            .Parameter("finishdate", finishDate)
        //            .GetDataTable(mr)
        //            .ToModels<RP_AGS>();
        //    }

        //    if (mr.error())
        //        ret.Exception = mr.ExceptionString;

        //    ret.Pf = DateTime.Now;
        //    return ret;
        //}




        //[HttpPost]
        //[HttpGet]
        //[Route("getdashdatamonth")]
        //public ApiMethodReturn<AracGiris> GetDashBoardDataMonth(decimal hashservisid, int year, int month)
        //{
        //    MethodReturn mr = new MethodReturn();

        //    ApiMethodReturn<AracGiris> ret = new ApiMethodReturn<AracGiris>();

        //    DateTime startDate = new DateTime(year, month, 1);
        //    DateTime finishDate = startDate.endOfMonth();

        //    using (SasonWebAppPool appPool = SasonWebAppPool.Create)
        //    {
        //        ret.Data = appPool.EbaTestConnector.CreateQuery(@"
        //            SELECT 
        //                    sum(AGS) ARACGIRISSAYISI, sum(IES) ISEMRISAYISI, sum(ISLS) ISLEMSAYISI
        //            FROM 
        //                    RP_AGS 
        //            WHERE 
        //                    HASHSERVISID = {hashservisid}
        //                    AND TARIH BETWEEN {startdate} and {finishdate}")
        //            .Parameter("hashservisid", hashservisid)
        //            .Parameter("startdate", startDate)
        //            .Parameter("finishdate", finishDate)
        //            .GetDataTable(mr)
        //            .ToModel<AracGiris>();
        //    }

        //    if (mr.error())
        //        ret.Exception = mr.ExceptionString;

        //    ret.Pf = DateTime.Now;
        //    return ret;
        //}




        //public class CUSTOMER
        //{
        //    public string AD { get; set; }
        //    public string MUSTERIAD { get; set; }
        //    public string MUSTERITELEFON { get; set; }
        //    public string SASENO { get; set; }
        //    public string PLAKA { get; set; }
        //}


        //[HttpPost]
        //[HttpGet]
        //[Route("getactivecustomers")]
        //public ApiMethodReturn<List<CUSTOMER>> GetActiveCustomers(decimal hashservisid)
        //{
        //    MethodReturn mr = new MethodReturn();

        //    ApiMethodReturn<List<CUSTOMER>> ret = new ApiMethodReturn<List<CUSTOMER>>();

        //    DateTime startDate = DateTime.Now.AddDays(-1).startOfDay();
        //    DateTime finishDate = DateTime.Now.endOfDay();

        //    // trunc(sysdate-1)
        //    using (SasonWebAppPool appPool = SasonWebAppPool.Create)
        //    {
        //        ret.Data = appPool.EbaTestConnector.CreateQuery(@"
        //            SELECT distinct
        //                  o.ad,
        //                   S.MUSTERIAD,
        //                  S.MUSTERITELEFON,
        //                  s.saseno,R.PLAKA
        //                  from sason.vt_servisler t,sason.servisisemirler s,sason.vt_servisisortaklar o ,sason.servisvarlikruhsatlar r ,sason.vw_isortakkontakbilgiler h
        //                  where O.SERVISISORTAKID =S.SERVISISORTAKID and s.servisid=t.servisid
        //                  and t.dilkod='Turkish' and S.SASENO=R.SASENO
        //                  and trunc(s.kayittarih) between {startdate} and {finishdate}
        //                  and t.dilkod=o.dilkod 
        //                 and  ( sason.hashservisid(s.servisid) = {hashservisid})
        //                  and O.SERVISISORTAKID=h.servisisortakid(+)
        //                  and (h.servisisortakkontaktipid=6 or h.servisisortakkontaktipid is null)
        //            ")
        //            .Parameter("hashservisid", hashservisid)
        //            .Parameter("startdate",startDate)
        //            .Parameter("finishdate", finishDate)
        //            .GetDataTable(mr)
        //            .ToModels<CUSTOMER>();
        //    }

        //    if (mr.error())
        //        ret.Exception = mr.ExceptionString;

        //    ret.Pf = DateTime.Now;
        //    return ret;
        //}



        //public class ARACSATIS
        //{
        //    public decimal ADET { get; set; }
        //    public string TARIH { get; set; }
        //}


        //[HttpPost]
        //[HttpGet]
        //[Route("getaracsatis")]
        //public ApiMethodReturn<List<ARACSATIS>> GetAracSatis(decimal bayiid)
        //{
        //    MethodReturn mr = new MethodReturn();

        //    ApiMethodReturn<List<ARACSATIS>> ret = new ApiMethodReturn<List<ARACSATIS>>();

        //    DateTime startDate = DateTime.Now.startOfMonth();
        //    DateTime finishDate = startDate.endOfMonth();

        //    string startdatestr = $"{startDate.ToString("MM")}.{startDate.ToString("yyyy")}";
        //    string finishdatestr = $"{finishDate.ToString("MM")}.{finishDate.ToString("yyyy")}";

        //    //and tarih between { startdate} and { finishdate}
        //    using (SasonWebAppPool appPool = SasonWebAppPool.Create)
        //    {
        //        ret.Data = appPool.EbaTestConnector.CreateQuery(@"
        //            select * from mobilsatis where ownergroup = {ownergroup} 
        //            order by TARIH
        //            ")
        //            .Parameter("ownergroup", bayiid)
        //            //.Parameter("startdate", startdatestr)
        //            //.Parameter("finishdate", finishdatestr)
        //            .GetDataTable(mr)
        //            .ToModels<ARACSATIS>();
        //    }

        //    if (mr.error())
        //        ret.Exception = mr.ExceptionString;

        //    ret.Pf = DateTime.Now;
        //    return ret;
        //}






    }
}





/*
         //public class MOBILUSER
        //{
        //    public decimal ID { get; set; }
        //    public decimal SERVISID { get; set; }
        //    public string USERNAME { get; set; }
        //    public string SERVISNAME { get; set; }
        //    public decimal HASHSERVISID { get; set; }
        //    public decimal BAYIID { get; set; }
        //}

        //[HttpGet]
        //[HttpPost]
        //[Route("isuser")]
        //public ApiMethodReturn<MOBILUSER> IsUser(string username, string password)
        //{
        //    MethodReturn mr = new MethodReturn();
        //    ApiMethodReturn<MOBILUSER> ret = new ApiMethodReturn<MOBILUSER>();

        //    using (SasonWebAppPool appPool = SasonWebAppPool.Create)
        //    {
        //        ret.Data = appPool.EbaTestConnector.CreateQuery(@"
        //            SELECT * FROM MOBILUSER WHERE 
        //                    lower(USERNAME) = {username}
        //                    and lower(PASSWORD) = {password}
        //            ")
        //            .Parameter("username", username.ToLower())
        //            .Parameter("password", password.ToLower())
        //            .GetDataTable(mr)
        //            .ToModel<MOBILUSER>();

        //        if (mr.ok())
        //        {
        //            if (ret.Data.isNull())
        //                ret.Exception = "Kullanıcı veya Şifre Hatalı";
        //        }
        //        else
        //        {
        //            ret.Exception = mr.ExceptionString;
        //        }
        //    }

        //    if (mr.error())
        //        ret.Exception = mr.ExceptionString;

        //    return ret;
        //}







            public class RP_AGS
        {
            public Decimal ARACGIRISSAYISI { get; set; }
            public Decimal ISEMRISAYISI { get; set; }
            public Decimal ISLEMSAYISI { get; set; }
            public DateTime TARIH { get; set; }
            public string TARIHSTR { get { return $"{TARIH.ToString("dd")}/{TARIH.ToString("MM")}"; } }
            public decimal HASHSERVISID { get; set; }
            public decimal STOKTUTAR { get; set; }
        }




            //[HttpPost][HttpGet][Route("getdashdata")]
        //[ResponseCache(NoStore = true, Duration = 0)]
        //public ApiMethodReturn<RP_AGS> GetDashBoardData(decimal hashservisid)
        //{
        //    MethodReturn mr = new MethodReturn();

        //    ApiMethodReturn<RP_AGS> ret = new ApiMethodReturn<RP_AGS>();

        //    DateTime startDate = DateTime.Now.AddDays(-1).startOfDay();
        //    DateTime finishDate = DateTime.Now.endOfDay();

        //    using (SasonWebAppPool appPool = SasonWebAppPool.Create)
        //    {
        //        ret.Data = appPool.EbaTestConnector.CreateQuery(@"
        //            SELECT 
        //                    RPT.AGS ARACGIRISSAYISI, RPT.IES ISEMRISAYISI, RPT.ISLS ISLEMSAYISI, RPT.TARIH TARIH
        //                    ,STK.STOKTUTAR STOKTUTAR
        //            FROM 
        //                    RP_AGS RPT, MOBILSTOKTUTAR STK
        //            WHERE 
        //                    RPT.HASHSERVISID = {hashservisid}
        //                    AND STK.HASHSERVISID(+) = RPT.HASHSERVISID
        //                    and RPT.TARIH BETWEEN {startdate} and {finishdate}

        //            ")
        //            .Parameter("hashservisid", hashservisid)
        //            .Parameter("startdate", startDate)
        //            .Parameter("finishdate", finishDate)
        //            .GetDataTable(mr)
        //            .ToModel<RP_AGS>();
        //    }

        //    if (mr.error())
        //        ret.Exception = mr.ExceptionString;

        //    ret.Pf = DateTime.Now;
        //    return ret;
        //}




            [HttpPost][HttpGet][Route("getdashdatabetween")]
        public ApiMethodReturn<List<RP_AGS>> GetDashBoardDataThisMonth(decimal hashservisid)
        {
            MethodReturn mr = new MethodReturn();

            ApiMethodReturn<List<RP_AGS>> ret = new ApiMethodReturn<List<RP_AGS>>();

            DateTime startDate = DateTime.Now.AddDays(-1).startOfMonth();
            DateTime finishDate = DateTime.Now.endOfDay();

            using (SasonWebAppPool appPool = SasonWebAppPool.Create)
            {
                ret.Data = appPool.EbaTestConnector.CreateQuery(@"
                    SELECT 
                            AGS ARACGIRISSAYISI, IES ISEMRISAYISI, ISLS ISLEMSAYISI, TARIH
                    FROM 
                            RP_AGS 
                    WHERE 
                            HASHSERVISID = {hashservisid}
                            AND TARIH BETWEEN {startdate} and {finishdate}
                    ORDER BY TARIH
                    ")
                    .Parameter("hashservisid", hashservisid)
                    .Parameter("startdate", startDate)
                    .Parameter("finishdate", finishDate)
                    .GetDataTable(mr)
                    .ToModels<RP_AGS>();
            }

            if (mr.error())
                ret.Exception = mr.ExceptionString;

            ret.Pf = DateTime.Now;
            return ret;
        }
     */
