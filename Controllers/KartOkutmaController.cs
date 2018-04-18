using Antibiotic.Database.Field;
using Antibiotic.Extensions;
using Antibiotic.TableModels.TStructureModels.ReportTables;
using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SasonBase.Reports.KartOkutma.Base;
using SasonBase.Sason.Models.PdksModels;
using SasonBase.Sason.Models.TableModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SasonWebs.Controllers
{
    #region KartOkutmaSessionValues
    public class KartOkutmaSessionValues
    {
        private ISession Session { get; set; }

        public KartOkutmaSessionValues(ISession _session)
        {
            this.Session = _session;
        }

        public string SessionId { get => this.Session.Id; }
        public decimal ServisId { get => Session.GetInt32(nameof(this.ServisId)).cto<decimal>(); set => this.Session.SetInt32(nameof(this.ServisId), value.cto<int>()); }
        public decimal ServisTeknisyenId { get => Session.GetInt32(nameof(this.ServisTeknisyenId)) == null ? 0 : this.Session.GetInt32(nameof(this.ServisTeknisyenId)).Value.cto<decimal>(); set => this.Session.SetInt32(nameof(this.ServisTeknisyenId), value.cto<int>()); }
    } 
    #endregion

    public class KartOkutmaController : _Base._SasonController
    {
        #region ctor
        private IHostingEnvironment _env;
        public KartOkutmaController(IHostingEnvironment env)
        {
            _env = env;
        }
        #endregion

        #region Giris
        [HttpPost, HttpGet]
        [ResponseCache(NoStore = true, Duration = 0)]
        public IActionResult Giris(decimal stId)
        {
            //IActionResult result = null;
            MethodReturn mr = new MethodReturn();
            using (var ap = SasonWebAppPool.CreateMask)
            {
                SasonBase.Sason.Models.ReportModels.RptServisTeknisyen01 servisTeknisyen = R.Query<SasonBase.Sason.Models.ReportModels.RptServisTeknisyen01>(mr).First(t => t.ID == stId);
                if (mr.ok())
                {
                    KartOkutmaSessionValues mySession = new KartOkutmaSessionValues(HttpContext.Session);
                    mySession.ServisId = servisTeknisyen.SERVISID;
                    mySession.ServisTeknisyenId = servisTeknisyen.ID;
                    //result = View();
                }
            }

            //if (mr.error())
            //    result = RedirectToAction("DbConnectionError", "Genel", new { err = mr.ExceptionString }); //error sayfası yönlendirilecek

            return View("TeknisyenKartOkutma");
        }
        #endregion

        #region Kart Okutma Uygulama Methodları
        /// <summary>
        /// Teknisyenin Servisine Ait Açık İş Emirleri + Servisin Teknisyenlerinin Halen Devam Eden Çalışmalarına Ait İş Emirleri Getiriliyor
        /// </summary>
        /// <param name="loadOptions"></param>
        /// <returns></returns>
        [HttpGet, HttpPost]
        public object IsEmirler(DataSourceLoadOptions loadOptions)
        {
            KartOkutmaSessionValues session = new KartOkutmaSessionValues(HttpContext.Session);
            return DataSourceLoader.Load(Models.KartOkutma_IsEmirleri.GetData(session.ServisId, session.ServisTeknisyenId), loadOptions);
        }

        [HttpGet, HttpPost]
        public object IsEmirIslemler(decimal ISEMIRID, DataSourceLoadOptions loadOptions)
        {
            KartOkutmaSessionValues session = new KartOkutmaSessionValues(HttpContext.Session);
            return DataSourceLoader.Load(Models.KartOkutma_IsEmirIslemler.GetData(session.ServisId, ISEMIRID, session.ServisTeknisyenId), loadOptions);
        }

        [HttpGet, HttpPost]
        public IActionResult IslemYapabilirmi(decimal IsEmirId, decimal IsEmirIslemId)
        {
            MethodReturn mr = new MethodReturn();
            KartOkutmaSessionValues session = new KartOkutmaSessionValues(HttpContext.Session);
            decimal id = TEKNISYENHAREKET.IslemeGirisYapabilirMi(session.ServisId, session.ServisTeknisyenId, IsEmirId, IsEmirIslemId, mr);
            if (id < 0)
                return WmrException("Üzerinde Çalıştığınız İşlemden Çıkış Yapmadan Başka Bir İşleme Başlayamazsınız");
            else
                return WmrResult(new { HAREKETID = id }, mr);
        }

        [HttpGet, HttpPost]
        public object GetKartOkutmaNedenler(decimal isEmirIslemId, DataSourceLoadOptions loadOptions)
        {
            //Personelin Hareket Durumuna Göre Nedenlerin Getirililmesi
            KartOkutmaSessionValues session = new KartOkutmaSessionValues(HttpContext.Session);
            List<HareketNeden> nedenler = new List<HareketNeden>();
            using (var ap = SasonWebAppPool.CreateMask)
            {
                TEKNISYENHAREKET foundHareket = TEKNISYENHAREKET.GetAktifHareket(session.ServisTeknisyenId, isEmirIslemId);
                if (foundHareket.isNotNull())
                {
                    if (foundHareket.DURUM == YeniTeknisyenHareketDurum.Giris)
                    {
                        nedenler = R.Query<HareketNeden>().Where(t => t.HAREKETTIPI == HareketTipi.Cikis).ToList();
                        nedenler.forEach(neden => neden.NedenResim = "../images/waiting.png");
                        nedenler.forEach(neden =>
                        {
                            if (neden.FORMATI == NedenFormati.Cikis_IsBitisi)
                                neden.NedenResim = "../images/completed.png";
                        });
                    }
                    else if (foundHareket.DURUM == YeniTeknisyenHareketDurum.DevamBekliyor)
                    {
                        nedenler = R.Query<HareketNeden>().Where(t => t.FORMATI.In(NedenFormati.Giris_IsDevami)).ToList();
                        nedenler.forEach(neden => neden.NedenResim = "../images/nedengiris.png");
                    }
                }
                else
                {
                    List<TEKNISYENHAREKET> hareketler = TEKNISYENHAREKET.GetIsEmirIslemHareketleri(isEmirIslemId);
                    TEKNISYENHAREKET girisHareketi = hareketler.first(t => t.DURUM == YeniTeknisyenHareketDurum.Giris);
                    if (hareketler.isEmpty() || girisHareketi.isNull())
                        nedenler = R.Query<HareketNeden>().Where(t => t.FORMATI.In(NedenFormati.Giris_IsBaslangici)).ToList();
                    else
                        nedenler = R.Query<HareketNeden>().Where(t => t.FORMATI.In(NedenFormati.Giris_IsYardimi)).ToList();

                    nedenler.forEach(neden => neden.NedenResim = "../images/nedengiris.png");
                }
            }
            object result = DataSourceLoader.Load(nedenler, loadOptions);
            return result;
        }

        [HttpGet, HttpPost]
        public object UpdateTeknisyenHareket(decimal IsEmirId, decimal IsEmirIslemId, decimal UpdateHareketId, decimal NedenId, HareketTipi HareketTipi)
        {
            IActionResult result = null;
            MethodReturn mr = new MethodReturn();
            KartOkutmaSessionValues session = new KartOkutmaSessionValues(HttpContext.Session);
            using (var ap = SasonWebAppPool.CreateMask)
            {
                HareketNeden neden = R.Query<HareketNeden>().First(t => t.ID == NedenId);

                TEKNISYENHAREKET hareket = null;

                switch (HareketTipi)
                {
                    case HareketTipi.Giris:
                        {
                            hareket = new TEKNISYENHAREKET()
                            {
                                SERVISID = session.ServisId,
                                SERVISTEKNISYENID = session.ServisTeknisyenId,
                                ISEMIRID = IsEmirId,
                                ISEMIRISLEMID = IsEmirIslemId,
                                ISEMIRISLEMISCILIKID = 0,
                                GIRISTARIHI = DateTime.Now,
                                GIRISNEDENID = (int)neden.ID,
                                DURUM = YeniTeknisyenHareketDurum.Giris,
                            };

                            if (neden.FORMATI == NedenFormati.Giris_IsDevami)
                            {
                                TEKNISYENHAREKET oncekiHareket = TEKNISYENHAREKET.GetTeknisyenHareket(UpdateHareketId);
                                if (oncekiHareket.isNotNull())
                                {
                                    oncekiHareket.DURUM = YeniTeknisyenHareketDurum.Cikis;
                                    oncekiHareket.Update();
                                }
                            }
                        }
                        break;
                    case HareketTipi.Cikis:
                        {
                            hareket = TEKNISYENHAREKET.GetTeknisyenHareket(UpdateHareketId);
                            hareket.CIKISTARIHI = DateTime.Now;
                            hareket.CIKISNEDENID = (int)neden.ID;
                            if (neden.FORMATI != NedenFormati.Cikis_IsBitisi)
                                hareket.DURUM = YeniTeknisyenHareketDurum.DevamBekliyor;
                            else
                                hareket.DURUM = YeniTeknisyenHareketDurum.Cikis;
                        }
                        break;
                    default:
                        {
                            string err = "evet hata var. case'lere düşmesi gerekiyordu";
                        }
                        break;
                }

                mr = hareket.Update();
                result = WmrResult(hareket, mr);
            }
            return result;
        } 
        #endregion
        
        #region Raporlar İçin Method'lar
        /// <summary>
        /// Eba Tarafından Çağırılıyor...
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public IActionResult KartOkutmaRaporlari(string user)
        {
            MethodReturn mr = new MethodReturn();
            using (var ap = SasonBase.SasonBaseApplicationPool.CreateMask)
            {
                decimal servisId = ap.AppPool.EbaTestConnector.CreateQuery(@"
                        SELECT s.id, i.ad
                            FROM vw_firmalar f, vw_kullanicilar k, servisler s, isortaklar i
                            WHERE f.id = k.firmaid AND k.id = {userName} AND s.id = f.servisid and i.id = s.isortakid"
                    ).Parameter("userName", user).GetDataTable(mr).FirstRowFirstColumnValue().cto<decimal>();
                KartOkutmaSessionValues session = new KartOkutmaSessionValues(HttpContext.Session);
                session.ServisId = servisId;
                //ViewData["ServisId"] = servisId;
                //ViewData["User"] = user;
            }
            return View();
        }

        [HttpGet]
        [HttpPost]
        public IActionResult GetKartOkutmaRaporlari()
        {
            List<object> ret = new List<object>();
            typeof(KartOkutmaReporter).Assembly.findInheritedFrom(typeof(KartOkutmaReporter), false)
                .forEach(reporterType =>
                {
                    KartOkutmaReporter reporter = reporterType._create<KartOkutmaReporter>();
                    if (reporter.Disabled == false)
                        ret.add(new { Id = reporterType.FullName, Text = reporter.Text });
                });
            return WmrResult(ret, null);
        }

        [HttpGet]
        [HttpPost]
        public IActionResult GetReportDetails(string reportKey)
        {
            object ret = null;
            KartOkutmaReporter reporter = Assembly.GetAssembly(typeof(KartOkutmaReporter)).getType(reportKey)._create<KartOkutmaReporter>();
            if (reporter.isNotNull())
            {
                StringBuilder html = new StringBuilder();
                StringBuilder js = new StringBuilder();
                StringBuilder prms = new StringBuilder();
                int counter = 0;
                reporter.GetParameters.forEach(prm =>
                {
                    counter++;
                    html.Append(prm.Html);
                    js.Append(prm.Js);
                    if (counter > 1)
                        prms.Append(",");
                    prms.Append(prm.Name);
                });
                ret = new { ParameterNames = prms.ToString(), HtmlString = html.ToString(), JsString = js.ToString() };
            }
            return WmrResult(ret, null);
        }

        [HttpGet]
        [HttpPost]
        [ResponseCache(NoStore = true, Duration = 0)]
        public IActionResult ExecuteReport(string reportKey, string parameters)
        {
            KartOkutmaSessionValues session = new KartOkutmaSessionValues(HttpContext.Session);
            decimal servisId = session.ServisId;

            string exportPath = @"c:\eba.net\eba.net\attachments\temp";
#if DEBUG
            exportPath = Path.Combine(_env.WebRootPath, @"attachments\temp");
#endif
            Exception exception = null;
            object jsonClass = parameters.jsonToClass<object>(out exception);

            object ret = null;
            MethodReturn mr = new MethodReturn();
            KartOkutmaReporter reporter = Assembly.GetAssembly(typeof(KartOkutmaReporter)).getType(reportKey)._create<KartOkutmaReporter>();
            if (reporter.isNotNull())
            {
                reporter.ServisId = servisId;
                if (jsonClass != null)
                {
                    foreach (var item in (IEnumerable<object>)jsonClass)
                    {
                        string name = item.GetPropertyValue<string>("Name");
                        object value = item.GetPropertyValue<object>("Value").GetPropertyValue<object>("Value");
                        reporter.SetParameterIncomingValue(name, value);
                    }
                }

                //var    webRoot     = @"c:\eba.net\eba.net\attachments\temp";//  _env.WebRootPath;
                string pdfFile = $"{reporter.ReportFileCode}_{servisId}_{"USER"}.pdf";
                var pdfFilePath = System.IO.Path.Combine(exportPath, pdfFile);

                using (SasonWebAppPool appPool = SasonWebAppPool.Create)
                {
                    object reportDataSource = reporter.ExecuteReport(mr);
                    TReport report = TReport.SelectReportFromCode(reporter.SubjectCode);
                    if (report.isNotNull())
                    {
                        QueryReport.Models.TSubjectProperties props = new QueryReport.Models.TSubjectProperties();
                        props.AddFile(pdfFilePath);
                        mr = QueryReport.QueryReportAppPool.RunObjectReporter(report, reportDataSource, props);
                    }
                }

                if (mr.ok())
                    ret = new { PdfFile = $"/attachments/temp/{pdfFile}?user={DateTime.Now.ToString("yyyyMMddHHmmss")}" };
            }
            return WmrResult(ret, mr);
        }
        #endregion
    }

    namespace Models
    {
        #region KartOkutma_IsEmirleri
        public class KartOkutma_IsEmirleri //: SasonBase.Sason.Tables.Table_SERVISISEMIRLER.RawItem
        {
            public decimal ISEMIRID { get; set; }
            protected decimal SERVISARACID { get; set; }
            public Decimal SERVISID { get; set; }
            public Decimal TEKNIKOLARAKTAMAMLA { get; set; }

            public string ACIKLAMA { get; set; }
            public string ISEMIRNO { get; set; }
            public String PLAKA { get; set; }
            public DateTime KAYITTARIH { get; set; }

            public string IsEmirTarihiStr { get => KAYITTARIH.ToString("dd.MM.yyyy HH:mm"); }

            public int TEKNISYEN_HAREKET_DURUMU { get; set; }

            #region Teknisyen Durumu Resmi
            public string TeknisyenDurumResim
            {
                get
                {
                    string fileName = "";
                    switch ((YeniTeknisyenHareketDurum)TEKNISYEN_HAREKET_DURUMU)
                    {
                        case YeniTeknisyenHareketDurum.None:
                            fileName = "";
                            break;
                        case YeniTeknisyenHareketDurum.Giris:
                            //fileName = "../images/running.png";
                            fileName = "../images/nedengiris.png";
                            break;
                        case YeniTeknisyenHareketDurum.Cikis:
                            fileName = "../images/completed.png";
                            break;
                        case YeniTeknisyenHareketDurum.DevamBekliyor:
                            fileName = "../images/waiting.png";
                            break;
                    }
                    return fileName;
                }
            }
            #endregion

            public static List<KartOkutma_IsEmirleri> GetData(decimal servisId, decimal servisTeknisyenId)
            {
                MethodReturn mr = new MethodReturn();
                List<KartOkutma_IsEmirleri> result = null;
                using (var ap = SasonWebAppPool.CreateMask)
                {
                    // servis teknisyeninin önceki günlerde açık kalmış işlemi varsa kapatılacak
                    TEKNISYENHAREKET.CikisiYapilmamisHareketiKapat(servisId, servisTeknisyenId);

                    result = ap.AppPool.EbaTestConnector.CreateQuery($@"
                        select
                            isemir.*, 0 TEKNISYEN_HAREKET_DURUMU
                        from (select ID ISEMIRID, SERVISID, SERVISARACID, ISEMIRNO, ACIKLAMA, KAYITTARIH, PLAKA, TEKNIKOLARAKTAMAMLA from servisisemirler where servisid = {servisId} and (teknikolaraktamamla <> 1 or id in ( select isemirid from teknisyenhareket where servisid = {servisId} and servisteknisyenid = {servisTeknisyenId} and durum = 1 ))) isemir
                    ")
                    .GetDataTable(mr)
                    .ToModels<KartOkutma_IsEmirleri>(mr);

                    List<TEKNISYENHAREKET> hareketler = R.Query<TEKNISYENHAREKET>().Where(t => t.ISEMIRID.In(result.select(r => r.ISEMIRID)) && t.SERVISTEKNISYENID == servisTeknisyenId).ToList();

                    result.forEach(res =>
                    {
                        List<YeniTeknisyenHareketDurum> durumlar = hareketler.where(t => t.ISEMIRID == res.ISEMIRID).select(t=> t.DURUM).ToList();
                        if (durumlar.isNotEmpty())
                        {
                            if (durumlar.contains(YeniTeknisyenHareketDurum.Giris))
                                res.TEKNISYEN_HAREKET_DURUMU = 1;
                            else if (durumlar.contains(YeniTeknisyenHareketDurum.DevamBekliyor))
                                res.TEKNISYEN_HAREKET_DURUMU = 3;
                            else if (durumlar.contains(YeniTeknisyenHareketDurum.Cikis))
                                res.TEKNISYEN_HAREKET_DURUMU = 2;
                        }
                    });

                    result = result.orderByDesc(t => t.TEKNISYEN_HAREKET_DURUMU).toList();
                }
                return result;
            }
        }
        #endregion

        #region KartOkutma_IsEmirIslemler
        public class KartOkutma_IsEmirIslemler //: SasonBase.Sason.Tables.Table_SERVISISEMIRISLEMLER.RawItem
        {
            public decimal ISEMIRID { get; set; }
            public decimal ISLEMID { get; set; }
            public int SIRANO { get; set; }
            public string ACIKLAMA { get; set; }
            public int TEKNISYEN_HAREKET_DURUMU { get; set; }

            #region TeknisyenDurumResim
            public string TeknisyenDurumResim
            {
                get
                {
                    string fileName = "";
                    switch ((TeknisyenHareketDurum)TEKNISYEN_HAREKET_DURUMU)
                    {
                        case TeknisyenHareketDurum.None:
                            fileName = "";
                            break;
                        case TeknisyenHareketDurum.Giris:
                            //fileName = "../images/running.png";
                            fileName = "../images/nedengiris.png";
                            break;
                        case TeknisyenHareketDurum.Cikis:
                            fileName = "../images/completed.png";
                            break;
                        case TeknisyenHareketDurum.DevamBekliyor:
                            fileName = "../images/waiting.png";
                            break;
                    }
                    return fileName;
                }
            }
            #endregion

            public static List<KartOkutma_IsEmirIslemler> GetData(decimal servisId, decimal isemirId, decimal servisTeknisyenId)
            {
                MethodReturn mr = new MethodReturn();
                List<KartOkutma_IsEmirIslemler> result = null;
                using (var ap = SasonWebAppPool.CreateMask)
                {
                    result = ap.AppPool.EbaTestConnector.CreateQuery($@"
                        select
                            islem.*, 0 TEKNISYEN_HAREKET_DURUMU
                        from (select SERVISISEMIRID ISEMIRID, ID ISLEMID, SIRANO, ACIKLAMA from servisisemirislemler where servisisemirid = {isemirId}) islem
                        order by islem.sirano
                    ")
                    .GetDataTable(mr)
                    .ToModels<KartOkutma_IsEmirIslemler>(mr);

                    List<TEKNISYENHAREKET> hareketler = R.Query<TEKNISYENHAREKET>().Where(t => t.ISEMIRISLEMID.In(result.select(r => r.ISLEMID)) && t.SERVISTEKNISYENID == servisTeknisyenId).ToList();

                    result.forEach(res =>
                    {
                        List<YeniTeknisyenHareketDurum> durumlar = hareketler.where(t => t.ISEMIRISLEMID == res.ISLEMID).select(t => t.DURUM).ToList();
                        if (durumlar.isNotEmpty())
                        {
                            if (durumlar.contains(YeniTeknisyenHareketDurum.Giris))
                                res.TEKNISYEN_HAREKET_DURUMU = 1;
                            else if (durumlar.contains(YeniTeknisyenHareketDurum.DevamBekliyor))
                                res.TEKNISYEN_HAREKET_DURUMU = 3;
                            else if (durumlar.contains(YeniTeknisyenHareketDurum.Cikis))
                                res.TEKNISYEN_HAREKET_DURUMU = 2;
                        }
                    });

                }
                return result;
            }
        } 
        #endregion
    }


}