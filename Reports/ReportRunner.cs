//using Antibiotic.Extensions;
//using Antibiotic.TableModels.TStructureModels.ReportTables;
//using DevExpress.XtraPrinting;
//using DevExpress.XtraReports.UI;
//using QueryReport.Models;
//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Threading.Tasks;

//namespace SasonWebs.Reports
//{
//    public class ReportRunner
//    {
//        #region Run Report
//        public MethodReturn RunReport(TReport report, object reportDataSource, TSubjectProperties subjectProperties)
//        {
//            MethodReturn ret = new MethodReturn();
//            if (ret.ok())
//            {
//                Dictionary<string, XtraReport> xtraReports = new Dictionary<string, XtraReport>();

//                report.Pages.forEach(reportPage =>
//                {
//                    MethodReturn<XtraReport> xrMr = reportPage.toXtraReport();
//                    if (xrMr.ok())
//                    {
//                        xtraReports.set(reportPage.SF_Guid, xrMr.Result);
//                        xrMr.Result.DataSource = reportDataSource;
//                        xrMr.Result.CreateDocument();
//                    }
//                });

//                XtraReport mainReport = xtraReports.Values.first();
//                xtraReports.forEach(t =>
//                {
//                    if (!t.Value.Equals(mainReport))
//                        mainReport.Pages.AddRange(t.Value.Pages);
//                });

//                #region exportToFile
//                Func<string, XtraReportExportTypes, MethodReturn> exportToFile = delegate (string _toFileName, XtraReportExportTypes _exportType)
//                {
//                    MethodReturn _exportRet = new MethodReturn();
//                    try
//                    {
//                        switch (_exportType)
//                        {
//                            case XtraReportExportTypes.Xls:
//                                mainReport.ExportToXls(_toFileName);
//                                //rpt.PrintingSystem.PageCount
//                                break;
//                            case XtraReportExportTypes.Xlsx:
//                                mainReport.ExportToXlsx(_toFileName, new XlsxExportOptions(TextExportMode.Value, false, true, true));
//                                break;
//                            case XtraReportExportTypes.Csv:
//                                mainReport.ExportToCsv(_toFileName);
//                                break;
//                            case XtraReportExportTypes.Html:
//                                mainReport.ExportToHtml(_toFileName);
//                                break;
//                            case XtraReportExportTypes.Image:
//                                mainReport.ExportToImage(_toFileName);
//                                break;
//                            case XtraReportExportTypes.Pdf:
//                                mainReport.ExportToPdf(_toFileName);
//                                break;
//                            case XtraReportExportTypes.Rtf:
//                                mainReport.ExportToRtf(_toFileName);
//                                break;
//                            case XtraReportExportTypes.Text:
//                                mainReport.ExportToText(_toFileName);
//                                break;
//                            default:
//                                break;
//                        }
//                    }
//                    catch (Exception ex)
//                    {
//                        _exportRet.SetException(ex);
//                    }
//                    return _exportRet;
//                };

//                subjectProperties.exportFiles.forEach(export =>
//                {
//                    if (ret.ok())
//                        ret = exportToFile(export.Key, export.Value);
//                });
//                #endregion

//                if (mainReport.isNotNull())
//                    mainReport.Dispose();
//            }

//            return ret;
//        }
//        #endregion


//    }




//}
