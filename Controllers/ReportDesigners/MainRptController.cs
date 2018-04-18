//using DevExpress.Web.Mvc;
//using DevExpress.XtraReports.UI;
//using DevExpress.XtraReports.Web.Native.ClientControls.Services;
//using DevExpress.XtraReports.Web.ReportDesigner.Native;
//using DevExpress.XtraReports.Web.ReportDesigner.Native.Services;
//using Microsoft.AspNetCore.Hosting;
//using Microsoft.AspNetCore.Mvc;
//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace SasonWebs.Controllers.ReportDesigners
//{
//    public class MainRptController : Controller
//    {
//        public IActionResult ReportDesigner([FromServices] IReportDesignerModelGenerator reportDesignerModelGenerator, [FromServices] IJSContentGenerator<DevExpress.XtraReports.Web.ReportDesigner.ReportDesignerModel> reportDesignerJsContentGenerator, [FromServices] IHostingEnvironment hostingEnvironment)
//        {
//            XtraReport report = new XtraReport();
//            var globalDataSources = new Dictionary<string, object>();
//            //globalDataSources.Add("nwindDS", new SasonBase.Reports.XtraReport2().DataSource);

//            // Generate a server-side Report Designer model.
//            var designerModel = reportDesignerModelGenerator.Generate(report, globalDataSources, null, null, new ReportDesignerModelSettings());
//            // Assign routes to AJAX request handler actions.
//            designerModel.ReportDesignerHandlerUri = "/DXXRD";
//            designerModel.ReportPreviewHandlerUri = "/DXXRDV";
//            designerModel.QueryBuilderHandlerUri = "/DXQB";

//            // Generate the client-side designer model.
//            var stb = new StringBuilder();
//            reportDesignerJsContentGenerator.GenerateJson(stb, designerModel);
//            string modelString = stb.ToString();

//            // Obtain the Report Designer HTML template.
//            // The report-designer.html file is supplied with the DevExpress controls source code.
//            var templatesPath = Path.Combine(hostingEnvironment.WebRootPath, @"lib\xtrareportsjs\html\report-designer.html");
//            var htmlTemplates = System.IO.File.ReadAllText(templatesPath);

//            var model = new Models.ReportDesignerModel
//            {
//                ModelJson = modelString,
//                HtmlTemplates = htmlTemplates
//            };
//            return View(model);
//        }

//        public IActionResult ReportViewer()
//        {
//            var modelString = WebDocumentViewerExtension.GetJsonModelScript(new  SasonBase.Reports.XtraReport2(), "/DXXRDV");
//            var model = new Models.ClientControlModel
//            {
//                ModelJson = modelString
//            };
//            return View(model);
//        }

//        public IActionResult Error()
//        {
//            return View();
//        }
//    }
//}