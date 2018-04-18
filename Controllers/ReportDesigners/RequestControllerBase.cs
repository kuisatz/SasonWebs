//using DevExpress.XtraReports.Web.Native.ClientControls;
//using DevExpress.XtraReports.Web.Native.ClientControls.Services;
//using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.Extensions.Primitives;
//using SasonWebs.DevExpressOverrides;
//using System;
//using System.Collections.Generic;
//using System.Collections.Specialized;
//using System.Linq;
//using System.Threading.Tasks;

//namespace SasonWebs.Controllers.ReportDesigners
//{
//    public class RequestControllerBase : Controller
//    {
//        readonly IRequestManager requestManager;
//        public RequestControllerBase(IRequestManager requestManager)
//        {
//            this.requestManager = requestManager;
//        }
//        protected virtual async Task<IActionResult> Invoke()
//        {
//            IEnumerable<KeyValuePair<string, StringValues>> query = null;
//            if (HttpContext.Request.Method == "GET")
//                query = HttpContext.Request.Query;
//            else
//                query = await HttpContext.Request.ReadFormAsync();
//            NameValueCollection nameValueCollection = new NameValueCollection();
//            foreach (var pair in query)
//            {
//                nameValueCollection[pair.Key] = pair.Value;
//            }
//            var result = requestManager.ProcessRequest(nameValueCollection);
//            if (result is BinaryHttpActionResult)
//            {
//                var actionResult = (BinaryHttpActionResult)result;
//                return File(actionResult.Bytes, actionResult.ContentType, actionResult.FileName);
//            }
//            else
//            {
//                return new ActionResultWrapper(result);
//            }
//        }
//    }
//}
