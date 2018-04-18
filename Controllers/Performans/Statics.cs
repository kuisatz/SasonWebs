using Antibiotic.Extensions;
using SasonWebs.Controllers.Performans.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SasonWebs.Controllers.Performans
{
    public static class Statics
    {
        public static string Nf = "#,#0";

        public static object UserLockObject = new object();
        public static object MainCardsLockObject = new object();
        //public static object CardDetailsLockObject = new object();

        #region Users
        static Helpers.UserCollection Users = new Helpers.UserCollection();
        public static void SetServerUser(ServerUserInfo serverUserInfo)
        {
            Users.SetUserInfo(serverUserInfo);
        }

        public static ServerUserInfo GetServerUserInfo(string token)
        {
            return Users.GetUserInfo(token);
        }
        #endregion



        #region Servisler
        static Helpers.ServisCollection Servisler = new Helpers.ServisCollection();
        public static List<ServerServisInfo> GetServerServisler(List<decimal> servisIds)
        {
            return Servisler.GetServerServisler(servisIds);
        }

        public static List<ResponseServisInfo> GetResponseServisler(List<decimal> servisIds)
        {
            return Servisler.GetResponseServisler(servisIds);
        }
        #endregion


        #region Login Logs
        internal static object LogLockObject = new object();
        internal static object ErrLogLockObject = new object();
        public static void AppendToLog(ServerUserInfo serverUserInfo)
        {
            if (serverUserInfo.isNull())
                return;

            lock (LogLockObject)
            {
                FileInfo LogFile = new FileInfo($"__performans_log_{DateTime.Now.ToString("yyyy-MM-dd")}.txt");
                StringBuilder sb = new StringBuilder()
                    .Append(serverUserInfo.ClientIp.toString()).Append(", ")
                    .Append($@"[{serverUserInfo.StartDate.ToString("dd.MM.yyyy HH:mm:ss:fff")}]").Append(", ")
                    .Append($@"[{serverUserInfo.UserId} - {serverUserInfo.Language} - {serverUserInfo.UserName}]").Append(", ")
                    //.Append($@"[{serverUserInfo.ServisId} - {serverUserInfo.ServisHashId} - {serverUserInfo.ServisName}]").Append(", ")
                    .Append($@"[{serverUserInfo.LastUsedDate.ToString("dd.MM.yyyy HH:mm:ss:fff")}]").Append(", ");
                MethodReturn methodReturn = new MethodReturn();
                LogFile.appendToFile(sb.ToString(), methodReturn);
            }
        }

        public static void AppendToErrLogTokenNotFound(string token)
        {
            lock (ErrLogLockObject)
            {
                FileInfo LogFile = new FileInfo($@"__performans_err_log_{DateTime.Now.ToString("yyyy-MM-dd")}.txt");
                StringBuilder sb = new StringBuilder()
                    .Append("TOKEN:NOT_FOUND").Append(", ")
                    .Append($@"[Token : {token}]").Append(", ")
                    ;
                MethodReturn methodReturn = new MethodReturn();
                LogFile.appendToFile(sb.ToString(), methodReturn);
            }
        }

        public static void AppendToErrLog(ServerUserInfo serverUserInfo, string exceptionCode, string exception)
        {
            if (serverUserInfo.isNull())
                return;

            lock (ErrLogLockObject)
            {
                FileInfo LogFile = new FileInfo($@"__performans_err_log_{DateTime.Now.ToString("yyyy-MM-dd")}.txt");
                StringBuilder sb = new StringBuilder()
                    .Append(serverUserInfo.ClientIp.toString()).Append(", ")
                    .Append($@"[{serverUserInfo.StartDate.ToString("dd.MM.yyyy HH:mm:ss:fff")}]").Append(", ")
                    .Append($@"[{serverUserInfo.UserName} - {serverUserInfo.Password} - {serverUserInfo.Language}]").Append(", ")
                    .Append($@"[{exceptionCode}").Append(", ")
                    .Append($@"{exception}]");
                MethodReturn methodReturn = new MethodReturn();
                LogFile.appendToFile(sb.ToString(), methodReturn);
            }
        } 
        #endregion
    }
}