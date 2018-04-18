using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SasonWebs.Controllers.Genel
{
    public class GenelController : _Base._SasonController
    {
        public IActionResult Information()
        {
            return View();
        }

        public IActionResult WaitReadCardSimulate()
        {
            return View();
        }

        public IActionResult DbConnectionError(string err)
        {
            TempData["DbConnectionErr"] = err;
            return View();
        }
    }
}