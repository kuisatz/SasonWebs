using Antibiotic.Extensions;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SasonWebs.Controllers._Base
{
    public abstract class _SasonController : Controller
    {
        public IActionResult WmrException(string exception)
        {
            WebMethodReturn wmr = new WebMethodReturn() { Exception = exception };
            return Json(wmr);
        }

        public IActionResult WmrData(object data)
        {
            WebMethodReturn wmr = new WebMethodReturn() { Data = data };
            return Json(wmr);
        }

        public IActionResult WmrResult(object data, MethodReturn mr)
        {
            WebMethodReturn wmr = new WebMethodReturn() { Data = data };
            if (mr.isNotNull())
                wmr.Exception = mr.ExceptionString;
            return Json(wmr);
        }
    }
}