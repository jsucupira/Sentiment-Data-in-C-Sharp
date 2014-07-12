using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Common.Helpers
{
    public static class Utility
    {
        public static long ConvertDateTimeToTicks(this DateTime dtInput)
        {
            long ticks = 0;
            ticks = dtInput.Ticks;
            return ticks;
        }

        public static DateTime ConvertTicksToDateTime(this long lticks)
        {
            DateTime dtresult = new DateTime(lticks);
            return dtresult;
        }

    }
}
