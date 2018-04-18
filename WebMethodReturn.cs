using Antibiotic.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SasonWebs
{
    [Serializable()]
    public class WebMethodReturn
    {
        public bool Ok { get; set; } = true;

        #region [public] property (string): Exception
        private string _Exception;
        /// <summary>
        /// açıklama yok (string)
        /// </summary>
        public string Exception
        {
            get { return _Exception; }
            set
            {
                _Exception = value;
                this.Ok = _Exception.trim().isNullOrWhiteSpace();
            }
        }
        #endregion

        public object Data { get; set; }
    }
}
