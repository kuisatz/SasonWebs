using Antibiotic.Extensions;
using Microsoft.AspNetCore.Mvc;
using SasonWebs.Controllers.Performans.Models;
using SasonWebs.Models.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SasonWebs.Controllers.Performans
{
    public partial class PerformansController
    {
        [HttpGet, HttpPost, Route("getaftersalecards")]
        [ResponseCache(NoStore = true, Duration = 0)]
        public ApiMethodReturn<List<Card>> GetAfterSaleCards(string token)
        {
#if DEBUG
            CheckDebugUser();
#endif
            ApiMethodReturn<List<Card>> ret = new ApiMethodReturn<List<Card>>();
            lock (Statics.MainCardsLockObject)
            {
                using (var ap = SasonWebAppPool.CreateMask)
                {
                    ServerUserInfo userInfo = token.ToUser();
                    if (userInfo.isNotNull())
                    {
                        string hostMode = SasonBase.SasonConnectorManager.GetHostMode();

                        DateTime startDate = DateTime.Now.startOfDay();
                        DateTime finishDate = DateTime.Now.endOfDay();

                        if (ret.Ok)
                        {
                            ApiMethodReturn<List<Card>> tmr = Models.Sason.Sason_AcikIsEmirleri_Ozet.GetMainCards(userInfo);
                            if (tmr.Ok)
                                ret.Data.addRange(tmr.Data);
                            else
                                ret.Exception = tmr.Exception;
                        }

                        if (ret.Ok)
                        {
                            ApiMethodReturn<List<Card>> tmr = Models.Sason.Sason_AracGirisSayilari.GetMainCards(startDate, finishDate, userInfo.ServisIdler);
                            if (tmr.Ok)
                                ret.Data.addRange(tmr.Data);
                            else
                                ret.Exception = tmr.Exception;
                        }

                        if (ret.Ok)
                        {
                            ApiMethodReturn<List<Card>> tmr = Models.Sason.Sason_ServislerFatura.GetMainCards(userInfo.ServisIdler);
                            if (tmr.Ok)
                                ret.Data.addRange(tmr.Data);
                            else
                                ret.Exception = tmr.Exception;
                        }

                        if (ret.Ok)
                        {
                            ApiMethodReturn<List<Card>> tmr = Models.Sason.Sason_DownTime.GetMainCards(DateTime.Now.AddDays(-30).startOfDay(), DateTime.Now.endOfDay(), userInfo.ServisIdler);
                            if (tmr.Ok)
                                ret.Data.addRange(tmr.Data);
                            else
                                ret.Exception = tmr.Exception;
                        }

                        if (ret.Ok)
                        {
                            ApiMethodReturn<List<Card>> tmr = Models.Sason.Sason_ServislerCiro.GetMainCards(startDate, finishDate, userInfo.ServisIdler);
                            if (tmr.Ok)
                                ret.Data.addRange(tmr.Data);
                            else
                                ret.Exception = tmr.Exception;
                        }

                        if (ret.Ok)
                        {
                            ApiMethodReturn<List<Card>> tmr = Models.Sason.Sason_Musteriler.GetMainCards(startDate, finishDate, userInfo.ServisIdler);
                            if (tmr.Ok)
                                ret.Data.addRange(tmr.Data);
                            else
                                ret.Exception = tmr.Exception;
                        }

                        if (ret.Ok)
                        {
                            ApiMethodReturn<List<Card>> tmr = Models.Sason.Sason_YedekParca.GetMainCards(userInfo.ServisIdler);
                            if (tmr.Ok)
                                ret.Data.addRange(tmr.Data);
                            else
                                ret.Exception = tmr.Exception;
                        }
                    }
                    else
                    {
                        ret.Exception = "TOKEN:NOT_FOUND";
                        Statics.AppendToErrLogTokenNotFound(token);
                    }
                }
                ret.Pf = DateTime.Now;
            }
            return ret;
        }
    }
}