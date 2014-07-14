using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

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
                string currentLine = reader.ReadLine();
                while (!string.IsNullOrEmpty(currentLine))
                {
                    blobList.Add(currentLine);
                    currentLine = reader.ReadLine();
                }
            }
            return blobList;
        }

        public static Dictionary<string, string> StateList()
        {
            return new Dictionary<string, string>
            {
                {"AK", "alaska"},
                {"AL", "alabama"},
                {"AR", "arkansas"},
                {"AS", "american samoa"},
                {"AZ", "arizona"},
                {"CA", "california"},
                {"CO", "colorado"},
                {"CT", "connecticut"},
                {"DC", "district of columbia"},
                {"DE", "delaware"},
                {"FL", "florida"},
                {"GA", "georgia"},
                {"GU", "guam"},
                {"HI", "hawaii"},
                {"IA", "iowa"},
                {"ID", "idaho"},
                {"IL", "illinois"},
                {"IN", "indiana"},
                {"KS", "kansas"},
                {"KY", "kentucky"},
                {"LA", "louisiana"},
                {"MA", "massachusetts"},
                {"MD", "maryland"},
                {"ME", "maine"},
                {"MI", "michigan"},
                {"MN", "minnesota"},
                {"MO", "missouri"},
                {"MP", "northern mariana islands"},
                {"MS", "mississippi"},
                {"MT", "montana"},
                {"NA", "national"},
                {"NC", "north carolina"},
                {"ND", "north dakota"},
                {"NE", "nebraska"},
                {"NH", "new hampshire"},
                {"NJ", "new jersey"},
                {"NM", "new mexico"},
                {"NV", "nevada"},
                {"NY", "new york"},
                {"OH", "ohio"},
                {"OK", "oklahoma"},
                {"OR", "oregon"},
                {"PA", "pennsylvania"},
                {"PR", "puerto rico"},
                {"RI", "rhode island"},
                {"SC", "south carolina"},
                {"SD", "south dakota"},
                {"TN", "tennessee"},
                {"TX", "texas"},
                {"UT", "utah"},
                {"VA", "virginia"},
                {"VI", "virgin islands"},
                {"VT", "vermont"},
                {"WA", "washington"},
                {"WI", "wisconsin"},
                {"WV", "west virginia"},
                {"WY", "wyoming"}
            };
        }
    }
}