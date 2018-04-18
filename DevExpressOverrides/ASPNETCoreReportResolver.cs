//using System;
//using DevExpress.XtraReports.UI;
//using DevExpress.XtraReports.Web.WebDocumentViewer;

//namespace SasonWebs.DevExpressOverrides {
//    public class ASPNETCoreReportResolver : IWebDocumentViewerReportResolver {
//        public XtraReport Resolve(string reportEntry) {
//            if (reportEntry == "Categories") {
//                return new SasonBase.Reports.XtraReport2();
//            } else if (reportEntry == "Static") {
//                return new SasonBase.Reports.XtraReport1();
//            }
//            throw new ArgumentException("Could not found report with id: " + reportEntry);
//        }
//    }
//}