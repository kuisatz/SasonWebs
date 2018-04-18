//using System;
//using System.Collections.Concurrent;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using DevExpress.DataAccess.ConnectionParameters;
//using DevExpress.DataAccess.Sql;
//using DevExpress.XtraReports.Native;
//using DevExpress.XtraReports.UI;
//using DevExpress.XtraReports.Web.Extensions;
//using Microsoft.AspNetCore.Hosting;

//namespace SasonWebs.DevExpressOverrides {
//    public class CustomReportStorageWebExtension : ReportStorageWebExtension, IReportStorageTool2 {
//        readonly IHostingEnvironment hostingEnvironment;
//        ConcurrentDictionary<string, XtraReport> Reports { get; set; }
//        Dictionary<string, string> availableReports = new Dictionary<string, string>() {
//            { "Static", "Static Report" },
//            { "Categories", "Categories Report" }
//        };

//        public CustomReportStorageWebExtension(IHostingEnvironment hostingEnvironment) : base() {
//            this.hostingEnvironment = hostingEnvironment;
//            Reports = new ConcurrentDictionary<string, XtraReport>();
//        }

//        public override bool IsValidUrl(string url) {
//            return Reports.Keys.Contains(url) || availableReports.Keys.Contains(url);
//        }

//        public override string SetNewData(XtraReport report, string defaultUrl) {
//            this.SetData(report, defaultUrl);
//            return defaultUrl;
//        }

//        public override byte[] GetData(string url) {
//            XtraReport report;
//            if (!Reports.TryGetValue(url, out report))
//                report = GetReport(url, hostingEnvironment.ContentRootPath);
//            Reports.AddOrUpdate(url, report, (_url, _report) => { return report; });
//            using (MemoryStream stream = new MemoryStream()) {
//                report.SaveLayoutToXml(stream);
//                stream.Position = 0;
//                return stream.GetBuffer();
//            }
//        }

//        void IReportStorageTool2.AfterGetData(string url, XtraReport report) {
//            //Optional
//        }

//        public override Dictionary<string, string> GetUrls() {
//            var dictionary = new Dictionary<string, string>();
//            foreach (var report in Reports)
//                dictionary.Add(report.Key, report.Key);
//            foreach (var availableReport in availableReports) {
//                if (!dictionary.ContainsKey(availableReport.Key))
//                    dictionary.Add(availableReport.Key, availableReport.Value);
//            }
//            //var files = Directory.GetFiles(Path.Combine(hostingEnvironment.ContentRootPath, "Reports"));
//            //foreach(var item in files.Where(x => x.Contains(".repx")).Select(x => x.Split('\\').Last())) {
//            //    dictionary.Add(item, item);
//            //}
//            return dictionary;
//        }

//        public override void SetData(XtraReport report, string url) {
//            Reports.AddOrUpdate(url, report, (_key, _report) => { return report; });
//            //using (var fileStream = File.OpenWrite(Path.Combine(hostingEnvironment.ContentRootPath, @"Reports" + url))) {
//            //    report.SaveLayoutToXml(fileStream);
//            //}
//        }

//        public override bool CanSetData(string url) {
//            return true;
//        }

//        public static XtraReport GetReport(string reportName, string appBasePath) {
//            XtraReport report;
//            switch (reportName) {
//                case "Static": {
//                        report = new SasonBase.Reports.XtraReport1();
//                        break;
//                    }
//                case "Categories":
//                    {
//                        report = new SasonBase.Reports.XtraReport2();
//                        break;
//                    }
//                default: {
//                        throw new InvalidOperationException(string.Format("There is no report with the specified name: '{0}'", reportName));
//                    }
//            }

//            return report; 
//        }
//    }
//}