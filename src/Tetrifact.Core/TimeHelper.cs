using System;

namespace Tetrifact.Core
{
    public class TimeHelper
    {
        public static string ToIsoString(DateTime date)
        {
            string iso = date
                .ToLocalTime()
                .ToString("s") // convert to ymdhms
                .Replace("T", " "); // replace T after ymd

            return iso
                .Substring(0, iso.Length - 3); // remove sec
        }
    }
}
