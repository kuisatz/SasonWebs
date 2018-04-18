using Antibiotic.Extensions;
using Antibiotic.TableModels.TStructureModels.ReportTables;
//using DevExpress.XtraReports.UI;
using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Internal;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SasonBase.Reports.Teknisyen;
using SasonBase.Reports.Teknisyen.Base;
using SasonBase.Sason.Models.PdksModels;
using SasonBase.Sason.Models.ReportModels;
using SasonWebs.Models.Api;
//using SasonWebs.Reports;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SasonWebs.Controllers.Teknisyen
{
    public class TeknisyenController : _Base._SasonController
    {
        private IHostingEnvironment _env;

        public TeknisyenController(IHostingEnvironment env)
        {
            _env = env;
        }

        //http://localhost:59761/api/teknisyen/getservisteknisyenfromkartno?kartno=2E17A252
        [HttpGet][HttpPost][Route("api/teknisyen/getservisteknisyenfromkartno")][Produces("application/json")]
        public ApiMethodReturn<RptServisTeknisyen01> GetServisTeknisyenFromKartNo(string kartNo)
        {
            ApiMethodReturn<RptServisTeknisyen01> ret = new ApiMethodReturn<RptServisTeknisyen01>();
            MethodReturn internalRet = new MethodReturn();

            using (SasonWebAppPool appPool = SasonWebAppPool.Create)
                ret.Data = R.Query<RptServisTeknisyen01>(internalRet).First(t => t.KARTNO == kartNo && t.DURUMID == 1);

            if (internalRet.ok())
                if (ret.Data.isNull())
                    ret.Exception = "Teknisyen Bulunamadı veya Aktif Değil";

            if (internalRet.error())
                ret.Exception = internalRet.ExceptionString;

            ret.Pf = DateTime.Now;
            return ret;
        }

        [HttpGet][HttpPost]
        public IActionResult GetTeknisyenRaporlari()
        {
            List<object> ret = new List<object>();
            typeof(TeknisyenReporter).Assembly.findInheritedFrom(typeof(TeknisyenReporter), false)
                .forEach(reporterType =>
                {
                    TeknisyenReporter reporter = reporterType._create<TeknisyenReporter>();
                    if (reporter.Disabled == false)
                        ret.add(new { Id = reporterType.FullName, Text = reporter.Text });
                });
            return WmrResult(ret, null);
        }

        [HttpGet][HttpPost]
        public IActionResult GetReportDetails(string reportKey)
        {
            object ret = null;
            TeknisyenReporter reporter = Assembly.GetAssembly(typeof(TeknisyenReporter)).getType(reportKey)._create<TeknisyenReporter>();
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

        [HttpGet][HttpPost][ResponseCache(NoStore =true, Duration = 0)]
        public IActionResult ExecuteReport(string reportKey, decimal servisId, string parameters)
        {
            string exportPath = @"c:\eba.net\eba.net\attachments\temp";
#if DEBUG
            exportPath = Path.Combine(_env.WebRootPath, @"attachments\temp");
#endif
            Exception exception = null;
            object jsonClass = parameters.jsonToClass<object>(out exception);

            object ret = null;
            MethodReturn mr = new MethodReturn();
            TeknisyenReporter reporter = Assembly.GetAssembly(typeof(TeknisyenReporter)).getType(reportKey)._create<TeknisyenReporter>();
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
                string pdfFile     = $"{reporter.ReportFileCode}_{servisId}_{"USER"}.pdf";
                var    pdfFilePath = System.IO.Path.Combine(exportPath, pdfFile);

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


        [HttpPost][HttpGet]
        public IActionResult GetServisTeknisyenListesi(decimal servisId)
        {
            using (SasonWebAppPool appPool = SasonWebAppPool.Create)
                return WmrResult(R.Query<RptServisTeknisyen01>().Where(t => t.SERVISID == servisId).ToList(), null);
        }


        //[Attributes.TeknisyenControlAttribute]
        public IActionResult Giris(decimal stId)
        {
            IActionResult result = null;
            MethodReturn mr = new MethodReturn();
            using (SasonWebAppPool appPool = SasonWebAppPool.Create)
            {
                RptServisTeknisyen01 servisTeknisyen = R.Query<RptServisTeknisyen01>(mr).First(t => t.ID == stId);
                if (mr.ok())
                {
                    SasonWebSession mySession = new SasonWebSession(HttpContext.Session);
                    TempData["TeknisyenAdi"] = servisTeknisyen.AdSoyad;
                    mySession.ServisTeknisyenId = servisTeknisyen.ID.cto<int>();
                    mySession.ServisTeknisyenAdiSoyadi = servisTeknisyen.AdSoyad;
                    mySession.ServisId = servisTeknisyen.SERVISID.cto<int>();

                    SasonBase.Sason.Models.PdksModels.HareketUstBilgi.RepairServisIsemirleri(servisTeknisyen.SERVISID);

                    this.SetTeknisyenAdi();
                    result = View();
                }
            }

            if (mr.error())
                result = RedirectToAction("DbConnectionError", "Genel", new { err=mr.ExceptionString }); //error sayfası yönlendirilecek

            return RedirectToAction("IsEmirIslemleri");
        }

        public IActionResult IsemirIslemleri()
        {
            this.SetTeknisyenAdi();
            return View();
        }

        public ActionResult RunMenu(int menuId)
        {
            string actionName = "";
            string controllerName = "Teknisyen";

            switch (menuId)
            {
                case 1:
                    actionName = "IsEmirIslemleri";
                    break;
            }

            return RedirectToAction(actionName, controllerName);
        }

        /// <summary>
        /// Eba Tarafından Çağırılıyor...
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public IActionResult TeknisyenRaporlari(string user)
        {
            MethodReturn mr = new MethodReturn();
            using (SasonBase.SasonBaseApplicationPool appPool = SasonBase.SasonBaseApplicationPool.Create)
            {
                decimal servisId = appPool.EbaTestConnector.CreateQuery(@"
                        SELECT s.id, i.ad
                            FROM vw_firmalar f, vw_kullanicilar k, servisler s, isortaklar i
                            WHERE f.id = k.firmaid AND k.id = {userName} AND s.id = f.servisid and i.id = s.isortakid"
                    ).Parameter("userName", user).GetDataTable(mr).FirstRowFirstColumnValue().cto<decimal>();
                //SasonWebSession sasonWebSession = new SasonWebSession(this.HttpContext.Session);
                //sasonWebSession.ServisId = servisId;
                ViewData["ServisId"] = servisId;
                ViewData["User"] = user;
            }
            return View();
        }


    }
}