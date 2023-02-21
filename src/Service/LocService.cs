using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TigerL10N.Service
{
    public class LocService
    {
        public static CultureInfo[] GetAllCultures()
        {
            CultureInfo[] cinfo = CultureInfo.GetCultures(CultureTypes.AllCultures & ~CultureTypes.NeutralCultures);
            return cinfo;
        }
    }
}
