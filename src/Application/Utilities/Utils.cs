using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Utilities
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

        public static bool AnySourceContainsIgnoreCase(string content, params string?[] sources)
        {
            var contentToLower = content.ToLower();
            return sources.Any(source => source != null && source.ToLower().Contains(contentToLower));
        }

        public static bool EqualsAnyIgnoreCase(string value, params string[] patterns)
        {
            var valueToLower = value.ToLower();
            return patterns.Any(pattern => pattern.ToLower().Equals(valueToLower));
        }

        public static bool EndsWithIgnoreCase(string source, string content)
        {
            return source.ToLower().EndsWith(content.ToLower());
        }
    }
}
