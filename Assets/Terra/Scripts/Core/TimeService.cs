using System;

namespace TerraTactix.Core
{
    public static class TimeService
    {
        public static DateTime UtcNow => DateTime.UtcNow;

        public static string TodayKey => UtcNow.ToString("yyyy-MM-dd");

        public static TimeSpan UntilNextUtcDay
        {
            get
            {
                var next = UtcNow.Date.AddDays(1);
                var left = next - UtcNow;
                return left < TimeSpan.Zero ? TimeSpan.Zero : left;
            }
        }
    }
}
