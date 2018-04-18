//using DevExpress.XtraReports.Web.ReportDesigner.Native.Services;
//using Microsoft.AspNetCore.Hosting;
//using Microsoft.AspNetCore.Mvc;
//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Threading.Tasks;

//namespace SasonWebs.Controllers.ReportDesigners
//{
//    public class ReportDesignerController : RequestControllerBase
//    {
//        public ReportDesignerController(IReportDesignerRequestManager requestManager) : base(requestManager) { }

//        [Route("DXXRD")]
//        public Task<IActionResult> ReportDesignerInvoke()
//        {
//            return base.Invoke();
//        }

//        public IActionResult GetSupplementalJson([FromServices] IHostingEnvironment hostingEnvironment)
//        {
//            var supplementalPath = Path.Combine(hostingEnvironment.WebRootPath, @"js\CldrData\Supplemental.json");
//            return new JsonResult(System.IO.File.ReadAllText(supplementalPath));
//        }

//        public IActionResult GetCldrDataJson([FromServices] IHostingEnvironment hostingEnvironment)
//        {
//            var supplementalPath = Path.Combine(hostingEnvironment.WebRootPath, @"js\CldrData\en.json");
//            return new JsonResult(System.IO.File.ReadAllText(supplementalPath));
//        }
//    }
//}
