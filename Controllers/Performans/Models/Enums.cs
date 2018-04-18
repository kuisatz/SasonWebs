using Antibiotic.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SasonWebs.Controllers.Performans.Models
{
    [Flags()]
    public enum eTimes : int
    {
        Now, //Şimdi
        Last_7_Days, //Günlük
        Last_1_Month, //Haftalık
        Last_6_Months, //Aylık
        Last_1_Year, //Aylık
    }

    public enum eTimeGroup
    {
        Day,
        Week,
        Month,
        Year,
        Total
    }


    public enum eToplamTipi
    {
        ServisBazinda,
        Toplam
    }

    public static class EnumsHelper
    {
        public static List<string> EnumZamanTipiAdlari(params eTimes[] zamanTipleri)
        {
            List<string> ret = new List<string>();
            foreach (var zt in zamanTipleri)
                ret.add(Enum.GetName(typeof(eTimes), zt));
            return ret;
        }
    }
}