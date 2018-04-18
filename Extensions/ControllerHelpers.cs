using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public static class ControllerHelpers
{
    public static void SetTeknisyenAdi(this SasonWebs.Controllers.Teknisyen.TeknisyenController teknisyenController)
    {
        if (teknisyenController.HttpContext != null && teknisyenController.HttpContext.Session != null)
            teknisyenController.ViewData["TeknisyenAdi"] = new SasonWebs.SasonWebSession(teknisyenController.HttpContext.Session).ServisTeknisyenAdiSoyadi;
    }
}