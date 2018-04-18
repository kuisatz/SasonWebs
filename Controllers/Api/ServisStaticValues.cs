using SasonBase.Sason.Models.TableModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Antibiotic.Extensions;

namespace SasonWebs.Controllers.Api
{
    public static partial class ServisStaticValues
    {
        internal static object FaturaLockObject = new object();
        internal static object IrsaliyeLockObject = new object();



        static ServisStaticCollection servisStatics = new ServisStaticCollection();

        public static ServisInformation GetServisInformation(decimal servisId)
        {
            return servisStatics.GetServisInformation((int)servisId);
        }

        public static bool FaturaIzni(SERVISLERWS servislerWs)
        {
            return servisStatics.FaturaWsIzniVarmi(servislerWs);
        }

        public static bool IrsaliyeIzni(SERVISLERWS servislerWs)
        {
            return servisStatics.IrsaliyeWsIzniVarmi(servislerWs);
        }
    }

    public static partial class ServisStaticValues
    {
        internal static object LogLockObject = new object();

        public static void AddWsInfoToLog(SERVISLERWS servislerWs, string wsMethodName, DateTime ps, DateTime pf, int dataCount, string exception, List<string> parameterValues)
        {
            if (servislerWs.isNull())
                return;

            lock (LogLockObject)
            {
                //string today = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff");
                FileInfo LogFile = new FileInfo($@"__web_service_log_{DateTime.Now.ToString("yyyy-MM-dd")}.txt");
                StringBuilder sb = new StringBuilder()
                    .Append(ps.ToString("yyyy-MM-dd HH:mm:ss:fff")).Append(", ")
                    .Append(pf.ToString("yyyy-MM-dd HH:mm:ss:fff")).Append(", ")
                    .Append(servislerWs.SERVISID).Append(", ")
                    .Append(servislerWs.SERVISADI).Append(", ")
                    .Append(servislerWs.WSKODU).Append(", ")
                    .Append(servislerWs.WSSIFRE).Append(", ")
                    .Append(wsMethodName).Append(", ")
                    .Append(dataCount).Append(", ");
                    
                int count = 0;
                foreach (var tparam in parameterValues)
                {
                    if (count > 0)
                        sb.Append(", ");
                    sb.Append(tparam);
                    count++;
                }

                sb.Append(", ").Append(exception);

                MethodReturn methodReturn = new MethodReturn();
                LogFile.appendToFile(sb.ToString(), methodReturn);
            }
        }
    }


    class ServisStaticCollection : Antibiotic.Collections.Threads.Base.ThreadKeyValueCollectionBase<int, ServisInformation>
    {
        ServisInformation CheckServisInfo(int servisId)
        {
            ServisInformation servisInformation = base.secretGet(servisId);
            if (servisInformation == null)
            {
                servisInformation = new ServisInformation()
                {
                    ServisId = servisId
                };
                base.secretSet(servisId, servisInformation);
            }
            return servisInformation;
        }

        public ServisInformation GetServisInformation(int servisId)
        {
            Lock();
            ServisInformation servisInformation = CheckServisInfo(servisId);
            Unlock();
            return servisInformation;
        }

        public bool FaturaWsIzniVarmi(SERVISLERWS servislerWs)
        {
            Lock();
            ServisInformation servisInformation = CheckServisInfo((int)servislerWs.SERVISID);
            bool ret = (DateTime.Now - servisInformation.SonFaturaWsIstekCevapZamani).TotalMinutes >= (int)servislerWs.BEKLEMESURESI;
            if (ret)
                servisInformation.SonFaturaWsIstekCevapZamani = DateTime.Now;
            Unlock();
            return ret;
        }

        public bool IrsaliyeWsIzniVarmi(SERVISLERWS servislerWs)
        {
            Lock();
            ServisInformation servisInformation = CheckServisInfo((int)servislerWs.SERVISID);
            bool ret = (DateTime.Now - servisInformation.SonIrsaliyeWsIstekCevapZamani).TotalMinutes >= (int)servislerWs.BEKLEMESURESI;
            if (ret)
                servisInformation.SonIrsaliyeWsIstekCevapZamani = DateTime.Now;
            Unlock();
            return ret;
        }
    }

    public class ServisInformation
    {
        public decimal ServisId { get; set; }
        public DateTime SonFaturaWsIstekCevapZamani { get; set; }
        public DateTime SonIrsaliyeWsIstekCevapZamani { get; set; }
    }
}