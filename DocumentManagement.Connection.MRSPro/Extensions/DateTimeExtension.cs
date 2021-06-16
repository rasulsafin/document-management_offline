using System;

namespace MRS.DocumentManagement.Connection.MrsPro.Extensions
{
    internal static class DateTimeExtension
    {
        private static readonly DateTime DEFAULT_DATE =
            new (1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        internal static long ToUnixTime(this DateTime date)
        {
            var timeSpan = TimeZoneInfo.ConvertTimeToUtc(date)
                .Subtract(DEFAULT_DATE);
            return (long)timeSpan.TotalMilliseconds;
        }

        internal static DateTime? ToDateTime(this long unixDate)
        {
            if (unixDate == default)
                return null;

            DateTime dateTime = DEFAULT_DATE.AddMilliseconds(unixDate).ToLocalTime();
            return dateTime;
        }
    }
}
