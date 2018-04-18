//using System.IO;
//using DevExpress.XtraReports.Web.Native.ClientControls;
//using Microsoft.AspNetCore.Http;

//namespace SasonWebs.DevExpressOverrides {
//    public class WebApiHttpResponse : IHttpResponse {
//        readonly HttpResponse response;
//        public WebApiHttpResponse(HttpResponse response) {
//            this.response = response;
//        }

//        public int StatusCode {
//            get { return response.StatusCode; }
//            set { response.StatusCode = value; }
//        }

//        public string ContentType {
//            get { return response.ContentType; }
//            set { response.ContentType = value; }
//        }

//        public void AddHeader(string key, string value) {
//            response.Headers.Add(key, new[] { value });
//        }

//        public void BinaryWrite(byte[] bytes) {
//            using(MemoryStream stream = new MemoryStream(bytes)) {
//                stream.WriteTo(response.Body);
//            }
//        }

//        public async void Write(string content) {
//            await response.WriteAsync(content);
//        }
//    }
//}
