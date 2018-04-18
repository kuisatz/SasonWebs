using Antibiotic.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SasonWebs.Models.Api
{
    public class ApiMethodReturn<T>
    {
        public T                Data            { get; set; }
        public string           Exception       { get; set; }
        public string           Warning         { get; set; }
        public DateTime         Ps              { get; set; } = DateTime.Now;
        public DateTime         Pf              { get; set; } = DateTime.Now;
        public WebServiceInfo   Info            { get; set; } = new WebServiceInfo();

        string _ExceptionCode;
        public string ExceptionCode
        {
            get { return _ExceptionCode; }
            set {
                _ExceptionCode = value;
                if (_ExceptionCode.isNotNullOrWhiteSpace())
                    Exception = "Err.From.Code.";
            }
        }

        public ApiMethodReturn()
        {
            this.Data = typeof(T)._create<T>();
        }

        public bool Ok
        {
            get { return Exception.isNullOrWhiteSpace(); }
        }
    }

    public class WebServiceInfo
    {
        public string Info1 { get; set; }
        public string Info2 { get; set; }
        public string Info3 { get; set; }
        public string Info4 { get; set; }
        public string Info5 { get; set; }
    }
}