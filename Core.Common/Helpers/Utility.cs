using System;
using System.Collections.Generic;
using System.IO;
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
        public static IEnumerable<string> GetArrayFromString(this byte[] stringValue)
        {
            List<string> blobList = new List<string>();
            using (MemoryStream stream = new MemoryStream((stringValue)))
            using (StreamReader reader = new StreamReader(stream, Encoding.UTF8, true, stringValue.Length))
            {
                var currentLine = reader.ReadLine();
                while (!string.IsNullOrEmpty(currentLine))
                {
                    blobList.Add(currentLine);
                    currentLine = reader.ReadLine();
                }
            }
            return blobList;
        }

    }
}
