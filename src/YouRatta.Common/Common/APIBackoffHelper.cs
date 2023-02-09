using System;

namespace YouRatta.Common;

public static class APIBackoffHelper
{
    public static TimeSpan GetRandomBackoff(TimeSpan minTime, TimeSpan maxTime, TimeSpan? previousBackoff = null)
    {
        Random random = null;
        if (previousBackoff.HasValue)
        {
            random = new Random((Int32)previousBackoff.Value.TotalMilliseconds);
        }
        else
        {
            random = new Random();
        }

        return TimeSpan.FromMilliseconds(random.Next((Int32)minTime.TotalMilliseconds, (Int32)maxTime.TotalMilliseconds));
    }
}
