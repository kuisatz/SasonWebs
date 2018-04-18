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
        [HttpGet, HttpPost, Route("getsalecards")]
        [ResponseCache(NoStore = true, Duration = 0)]
        public ApiMethodReturn<List<Card>> GetSaleCards(string token)
        {
#if DEBUG
            CheckDebugUser();
#endif
            ApiMethodReturn<List<Card>> ret = new ApiMethodReturn<List<Card>>();
            lock (Statics.MainCardsLockObject)
            {
                using (var ap = new AppPoolMask<SasonWebAppPool>())
                {
                    if (ap.Custom)
                        ap.AppPool = SasonWebAppPool.Create;

                    ServerUserInfo userInfo = token.ToUser();
                    if (userInfo.isNotNull())
                    {
                        string hostMode = SasonBase.SasonConnectorManager.GetHostMode();

                        DateTime startDate = DateTime.Now.startOfDay();
                        DateTime finishDate = DateTime.Now.endOfDay();

                        if (ret.Ok)
                        {
                            ApiMethodReturn<List<Card>> tmr = Models.Crm.Merkez.Man_Stok.GetMainCards(userInfo.BayiIdler);
                            if (tmr.Ok)
                                ret.Data.addRange(tmr.Data);
                            else
                                ret.Exception = tmr.Exception;
                        }

                        if (ret.Ok)
                        {
                            ApiMethodReturn<List<Card>> tmr = Models.Crm.Bayi.Bayi_Stok.GetMainCards(userInfo.BayiIdler);
                            if (tmr.Ok)
                                ret.Data.addRange(tmr.Data);
                            else
                                ret.Exception = tmr.Exception;
                        }

                        if (ret.Ok)
                        {
                            ApiMethodReturn<List<Card>> tmr = Models.Crm.Bayi.Bayi_FaturaSayilar.GetMainCards(userInfo.BayiIdler);
                            if (tmr.Ok)
                                ret.Data.addRange(tmr.Data);
                            else
                                ret.Exception = tmr.Exception;
                        }

                        if (ret.Ok)
                        {
                            ApiMethodReturn<List<Card>> tmr = Models.Crm.Bayi.Bayi_Firsatlar.GetMainCards(userInfo.BayiIdler);
                            if (tmr.Ok)
                                ret.Data.addRange(tmr.Data);
                            else
                                ret.Exception = tmr.Exception;
                        }

                        if (ret.Ok)
                        {
                            ApiMethodReturn<List<Card>> tmr = Models.Crm.Bayi.Bayi_Aktivite.GetMainCards(userInfo.BayiIdler);
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