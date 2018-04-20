using Antibiotic.Extensions;
using Antibiotic.TableModels.TStructureModels.ReportTables;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using SasonBase.Sason.Models.ReportModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SasonWebs.Controllers
{
    public class SasonController : _Base._SasonController
    {
        private IHostingEnvironment _env;

        public SasonController(IHostingEnvironment env)
        {
            _env = env;
        }

        [HttpGet]
        [HttpPost]
        [ResponseCache(NoStore = true, Duration = 0)]
        public IActionResult Raporlar(string user)
        {
            MethodReturn mr = new MethodReturn();
            using (var ap = SasonWebAppPool.CreateMask)
            {
                decimal servisId = ap.AppPool.EbaTestConnector.CreateQuery(@"
                        SELECT s.id, i.ad
                            FROM vw_firmalar f, vw_kullanicilar k, servisler s, isortaklar i
                            WHERE f.id = k.firmaid AND k.id = {userName} AND s.id = f.servisid and i.id = s.isortakid
                    ")
                    .Parameter("userName", user)
                    .GetDataTable(mr)
                    .FirstRowFirstColumnValue().cto<decimal>();

                SasonWebSession session = new SasonWebSession(HttpContext.Session);
                session.ServisId = servisId;
                session.ServisTeknisyenAdiSoyadi = user.Replace(".", "");

                if (servisId == 1) //Merkez
                    session.ReporterBaseClassName = typeof(SasonBase.Reports.Sason.Merkez.Base.SasonMerkezReporter).FullName;
                else //Servis
                    session.ReporterBaseClassName = typeof(SasonBase.Reports.Sason.Servis.Base.SasonReporter).FullName;
            }
            return View();
        }

        [HttpPost]
        [HttpGet]
        public IActionResult GetManServisListesi()
        {
            using (var ap = SasonWebAppPool.CreateMask)
            {
                List<object> list = ap.AppPool.EbaTestConnector.CreateQuery($@"
                    select SERVISID ID, ISORTAKAD AD from vt_servisler where DURUMID = 1 AND dilkod = 'Turkish' order by id
                ")
                .GetDataTable()
                .ToModels();
                return WmrResult(list, null);
            }
        }

        [HttpPost]
        [HttpGet]
        public IActionResult GetReceteListesi()
        {
            using (var ap = SasonWebAppPool.CreateMask)
            {
                List<object> list = ap.AppPool.EbaTestConnector.CreateQuery($@"
                        SELECT ID,KOD FROM receteler
                        where durumID = 1  
                        order by kod desc
                ")
                .GetDataTable()
                .ToModels();
                return WmrResult(list, null);
            }
        }

        [HttpPost]
        [HttpGet]
        public IActionResult GetAgregaListesi()
        {
            using (var ap = SasonWebAppPool.CreateMask)
            {
                List<object> list = ap.AppPool.EbaTestConnector.CreateQuery($@" 
                    SELECT ID, q.AD as KOD from vw_agregalar q 
                    where Q.DILKOD = 'Turkish' AND 
                    q.durumID = 1  
                    order by q.AD
                ")
                .GetDataTable()
                .ToModels();
                return WmrResult(list, null);
            }
        }


        [HttpPost]
        [HttpGet]
        public IActionResult GetBakimGruplariListesi()
        {
            using (var ap = SasonWebAppPool.CreateMask)
            {
                List<object> list = ap.AppPool.EbaTestConnector.CreateQuery($@"
                       SELECT ID,KOD FROM bakimgruplar 
                        where durumID = 1  
                        order by kod
                ")
                .GetDataTable()
                .ToModels();
                return WmrResult(list, null);
            }
        }


        [HttpPost]
        [HttpGet]
        public IActionResult GetIsEmirHizmetYerleri()
        {
            List<object> result = new List<object>();
            result
                .add(new { ID = 1, AD = "Servis" })
                .add(new { ID = 2, AD = "Şantiye" })
                .add(new { ID = 3, AD = "Yol Yardımı" });
            return WmrResult(result, null);
        }

        private class ReporterInfo
        {
            public string Id { get; set; }
            public string Text { get; set; }
        }


        [HttpGet]
        [HttpPost]
        public IActionResult GetRaporlar()
        {
            SasonWebSession session = new SasonWebSession(HttpContext.Session);
            List<ReporterInfo> ret = new List<ReporterInfo>();
            Assembly asm = Assembly.GetAssembly(typeof(SasonBase.Reports.Reporter));
            asm.findInheritedFrom(asm.getType(session.ReporterBaseClassName), true)
                .forEach(reporterType =>
                {
                    SasonBase.Reports.Reporter reporter = reporterType._create<SasonBase.Reports.Reporter>();
                    if (reporter != null)
                    {
                        if (reporter.Disabled == false)
                            ret.add(new ReporterInfo { Id = reporterType.FullName, Text = reporter.Text });
                        //ret.add(new { Id = reporterType.FullName, Text = reporter.Text });
                    }
                });
            return WmrResult(ret.orderBy(t=> t.Text).toList(), null);
        }

        [HttpGet]
        [HttpPost]
        public IActionResult GetReportDetails(string reportKey)
        {
            SasonWebSession session = new SasonWebSession(HttpContext.Session);
            Assembly asm = Assembly.GetAssembly(typeof(SasonBase.Reports.Reporter));

            object ret = null;
            SasonBase.Reports.Reporter reporter = asm.getType(reportKey)._create<SasonBase.Reports.Reporter>();
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
            SasonWebSession session = new SasonWebSession(HttpContext.Session);
            Assembly asm = Assembly.GetAssembly(typeof(SasonBase.Reports.Reporter));

            decimal servisId = session.ServisId;
            string userName = session.ServisTeknisyenAdiSoyadi;


            string exportPath = @"c:\eba.net\eba.net\attachments\temp";
#if DEBUG
            exportPath = Path.Combine(_env.WebRootPath, @"attachments\temp");
#endif
            Exception exception = null;
            object jsonClass = parameters.jsonToClass<object>(out exception);

            object ret = null;
            MethodReturn mr = new MethodReturn();
            SasonBase.Reports.Reporter reporter = asm.getType(reportKey)._create<SasonBase.Reports.Reporter>();
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

                string pdfFile = $"{reporter.ReportFileCode}_{userName}.pdf";
                string excelFile = $"{reporter.ReportFileCode}_{userName}.xlsx";
                var pdfFilePath = System.IO.Path.Combine(exportPath, pdfFile);
                var excelFilePath = System.IO.Path.Combine(exportPath, excelFile);

                using (var ap = SasonWebAppPool.CreateMask)
                {
                    object reportDataSource = reporter.ExecuteReport(mr);
                    TReport report = TReport.SelectReportFromCode(reporter.SubjectCode);
                    if (report.isNotNull())
                    {
                        QueryReport.Models.TSubjectProperties props = new QueryReport.Models.TSubjectProperties();
                        props.AddFile(pdfFilePath);
                        props.AddFile(excelFilePath);
                        mr = QueryReport.QueryReportAppPool.RunObjectReporter(report, reportDataSource, props);
                    }
                }

                if (mr.ok())
                    ret = new {
                        PdfFile = $"/attachments/temp/{pdfFile}?user={DateTime.Now.ToString("yyyyMMddHHmmss")}",
                        ExcelFile = $"/attachments/temp/{excelFile}?user={DateTime.Now.ToString("yyyyMMddHHmmss")}",
                        ExcelServerSideFileName = excelFilePath,
                        ExcelClientSideFileName = $"{DateTime.Now.ToString("yyyy-MM-dd")}_{reporter.ReportFileCode}_{userName}.xlsx"
                    };
            }
            return WmrResult(ret, mr);
        }

        [HttpGet]
        public virtual ActionResult DownloadFile(string serverSideFileName, string clientFileName)
        {
            Exception exception = null;
            byte[] data = new FileInfo(serverSideFileName).readAllBytes(out exception);
            return File(data, "application/vnd.ms-excel", clientFileName);
        }
    }
}