//using DevExpress.XtraReports.Web.Native.ClientControls;
//using Microsoft.AspNetCore.Mvc;

//namespace SasonWebs.DevExpressOverrides {
//    public class ActionResultWrapper : ActionResult {
//        readonly protected HttpActionResultBase response;
//        public ActionResultWrapper(HttpActionResultBase response) {
//            this.response = response;
//        }

//        public override void ExecuteResult(ActionContext actionContext) {
//            response.Write(new WebApiHttpResponse(actionContext.HttpContext.Response));
//        }
//    }
//}