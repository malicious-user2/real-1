using System;

namespace YouRata.Common;

public static class APIBackoffHelper
{
    public static TimeSpan GetRandomBackoff(TimeSpan minTime, TimeSpan maxTime)
    {
        Random random = new Random();
        return TimeSpan.FromMilliseconds(random.Next((Int32)minTime.TotalMilliseconds, (Int32)maxTime.TotalMilliseconds));
    }
}
