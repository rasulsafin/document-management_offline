using System;

namespace Brio.Docs.Connections.MrsPro.Extensions
{
    internal static class DateTimeExtensions
    {
        internal static long ToUnixTime(this DateTime dateTime)
        {
            var offset = dateTime.ToUniversalTime() <= DateTimeOffset.MinValue.UtcDateTime
                   ? DateTimeOffset.MinValue
                   : new DateTimeOffset(dateTime);

            return offset.ToUnixTimeMilliseconds();
        }

        internal static DateTime? ToLocalDateTime(this long unixDate)
        {
            if (unixDate == default)
                return null;

            var dateTime = DateTimeOffset.FromUnixTimeMilliseconds(unixDate);
            return dateTime.LocalDateTime;
        }
    }
}
