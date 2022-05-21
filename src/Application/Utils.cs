using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application
{
    public static class Utils
    {
        public enum DateTimeUnit { MINTUES, DAYS };

        public static bool IsWithinTimeframe(DateTime from, int timeframeLength, DateTimeUnit timeframeUnit)
        {
            switch (timeframeUnit)
            {
                case DateTimeUnit.MINTUES:
                    return DateTime.UtcNow.Subtract(from).TotalMinutes < timeframeLength;

                case DateTimeUnit.DAYS:
                    return DateTime.UtcNow.Subtract(from).TotalDays < timeframeLength;

                default:
                    throw new ArgumentException("Unknown timeframeUnit value");
            }
        }
    }
}
