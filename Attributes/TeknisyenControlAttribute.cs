using Antibiotic.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using SasonBase.Sason.Models.ReportModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SasonWebs.Attributes
{
    public class TeknisyenControlAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            SasonWebSession session= new SasonWebSession(context.HttpContext.Session);
            //session.TeknisyenId = context.ActionArguments.cast<Dictionary<string, object>>().find("stId").cto<int>();
            //mySession.TeknisyenId = 
            base.OnActionExecuting(context);
        }
        
        //[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
        //public sealed class NoCacheAttribute : ActionFilterAttribute
        //{
        //    public override void OnResultExecuting(ResultExecutingContext filterContext)
        //    {
        //        filterContext.HttpContext.Response.Cache.SetExpires(DateTime.UtcNow.AddDays(-1));
        //        filterContext.HttpContext.Response.Cache.SetValidUntilExpires(false);
        //        filterContext.HttpContext.Response.Cache.SetRevalidation(HttpCacheRevalidation.AllCaches);
        //        filterContext.HttpContext.Response.Cache.SetCacheability(HttpCacheability.NoCache);
        //        filterContext.HttpContext.Response.Cache.SetNoStore();

        //        base.OnResultExecuting(filterContext);
        //    }
        //}
    }
}