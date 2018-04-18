using Antibiotic.Extensions;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SasonWebs
{
    public class SasonWebSession
    {
        private ISession Session { get; set; }

        public SasonWebSession(ISession _session)
        {
            this.Session = _session;
        }

        public string SessionId
        {
            get { return this.Session.Id; }
        }

        public int ServisTeknisyenId
        {
            get
            {
                return Session.GetInt32(nameof(this.ServisTeknisyenId)) == null ? 0 : this.Session.GetInt32(nameof(this.ServisTeknisyenId)).Value;
            }
            set
            {
                this.Session.SetInt32(nameof(this.ServisTeknisyenId), value);
            }
        }

        public string ServisTeknisyenAdiSoyadi
        {
            get
            {
                return Session.GetString(nameof(this.ServisTeknisyenAdiSoyadi)).toString();
            }
            set { this.Session.SetString(nameof(this.ServisTeknisyenAdiSoyadi), value); }
        }

        public decimal ServisId
        {
            get
            {
                return Session.GetInt32(nameof(this.ServisId)).cto<decimal>();
            }
            set
            {
                this.Session.SetInt32(nameof(this.ServisId), value.cto<int>());
            }
        }

        public string ReporterBaseClassName
        {
            get
            {
                return Session.GetString(nameof(this.ReporterBaseClassName)).toString();
            }
            set { this.Session.SetString(nameof(this.ReporterBaseClassName), value); }
        }
    }
}