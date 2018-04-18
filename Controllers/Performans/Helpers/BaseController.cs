using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SasonWebs.Controllers.Performans.Helpers
{
    public class BaseController : Controller
    {
        protected IHostingEnvironment _env;

        public BaseController()
        {

        }

        public BaseController(IHostingEnvironment env)
        {

        }
    }
}