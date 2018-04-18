using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SasonWebs.Controllers.Performans.Models
{
    public class ResponseUserInfo
    {
        public string Token { get; set; }
        public string ServisName { get; set; }
        public string UserName { get; set; }
        public string Language { get; set; }
        public string Name { get; set; }
        public string UserImage { get; set; }

        public List<ResponseServisInfo> Servisler { get; set; }

        public ResponseUserInfo()
        {
            Token = System.Guid.NewGuid().ToString("N");
        }
    }

    public class ServerUserInfo : ResponseUserInfo
    {
        public string Password { get; set; }
        public decimal UserId { get; set; }
        public List<decimal> BayiIdler { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime LastUsedDate { get; set; }
        public TimeSpan TimeElapsed { get => DateTime.Now - LastUsedDate; }

        public string ClientIp { get; set; }

        public List<Decimal> ServisIdler { get; set; }

        public ServerUserInfo() : base()
        {
            StartDate = DateTime.Now;
            LastUsedDate = DateTime.Now;
        }
    }
}