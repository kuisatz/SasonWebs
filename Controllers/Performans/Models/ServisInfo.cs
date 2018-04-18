using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SasonWebs.Controllers.Performans.Models
{
    public class ResponseServisInfo
    {
        public decimal ServisId { get; set; }
        public string ServisAdi { get; set; }
    }

    public class ServerServisInfo : ResponseServisInfo
    {
        //public decimal HashServisId { get; set; }
    }
}
