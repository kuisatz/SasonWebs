using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Antibiotic.Extensions;
using SasonWebs.Controllers.Performans.Models;
using System.IO;

namespace SasonWebs.Controllers.Performans.Helpers
{
    public class UserCollection : Antibiotic.Collections.Threads.Base.ThreadKeyValueCollectionBase<string, ServerUserInfo> //token, userinfo
    {
        /// <summary>
        /// logon olduktan sonra burası tetiklenecek
        /// </summary>
        /// <param name="userId"></param>
        //private void RemoveFromUserId(decimal userId)
        //{
        //    List<string> tokens = new List<string>();
        //    foreach (var tItem in dict)
        //    {
        //        if (tItem.Value.UserId == userId)
        //            tokens.Add(tItem.Key);
        //    }
        //    tokens.forEach(t => base.secretRemove(t));
        //}

        private void RemoveTimeOutTokens()
        {
            List<string> removeTokens = new List<string>();
            foreach (var tItem in dict)
            {
                if (tItem.Value.TimeElapsed.TotalDays > 5)
                    removeTokens.add(tItem.Key);
            }
            removeTokens.forEach(t => base.secretRemove(t));
        }

        public void SetUserInfo(ServerUserInfo userInfo)
        {
            Lock();

            //MethodReturn mr = new MethodReturn();
            //FileInfo fi = new FileInfo("_users.json");

            //if (base.dict.Count == 0 && fi.Exists)
            //{
            //    List<ServerUserInfo> list = fi.readAllText().jsonToClass<List<ServerUserInfo>>(mr);
            //    if (mr.ok())
            //    {
            //        list.forEach(x =>
            //        {
            //            base.secretSet(x.Token, x);
            //        });
            //    }
            //}

            //RemoveFromUserId(userInfo.UserId);
            //token = ip adresi geliyor
            RemoveTimeOutTokens();
            base.secretSet(userInfo.Token, userInfo);
            Statics.AppendToLog(userInfo);

            //string usersJsonData = base.dict.values().toJson();
            //fi.writeToFile(usersJsonData, mr);
            //if (mr.error())
            //{

            //}

            Unlock();
        }

        public ServerUserInfo GetUserInfo(string token)
        {
            base.Lock();

            //MethodReturn mr = new MethodReturn();
            //FileInfo fi = new FileInfo("_users.json");

            //if (base.dict.Count == 0 && fi.Exists)
            //{
            //    List<ServerUserInfo> list = fi.readAllText().jsonToClass<List<ServerUserInfo>>(mr);
            //    if (mr.ok())
            //    {
            //        list.forEach(x =>
            //        {
            //            base.secretSet(x.Token, x);
            //        });
            //    }
            //}

            ServerUserInfo ret = base.secretGet(token);
            if (ret.isNotNull())
            {
                ret.LastUsedDate = DateTime.Now;
            }

            RemoveTimeOutTokens();

            base.Unlock();
            return ret;
        }
    }
}