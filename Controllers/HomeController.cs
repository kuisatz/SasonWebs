using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace SasonWebs.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
#if DEBUG
            //using (SasonWebAppPool appPool = SasonWebAppPool.Create)
            //{
            //    ViewData["DbServer"] = appPool.EbaTestConnector.ConnectionSettings.Server.SF_ServerName;
            //    ViewData["DbUser"] = appPool.EbaTestConnector.ConnectionSettings.User.SF_UserName;
            //}

            //return View();
            //return new TeknisyenController().Test();

            //ViewData["DbServer"] = "Miya.Sason.WebService.v1_0";
            //ViewData["DbUser"] = "internalcoder";
            //return View();

            //return RedirectToAction("Test", "Teknisyen");//, new { area = "Admin" });
            //return RedirectToAction("IsEmirIslemleri", "Teknisyen");//, new { area = "Admin" });

            //return RedirectToAction("TeknisyenRaporlari", "Teknisyen", new { user = "aziz.iba" });
            //return RedirectToAction("Giris", "Teknisyen", new { stId = 26 });

            //http://localhost:59761/Teknisyen/TeknisyenRaporlari?user=mustafa

            return RedirectToAction("Information", "Genel");
#else
            return RedirectToAction("Information", "Genel");
#endif
        }


    }
}