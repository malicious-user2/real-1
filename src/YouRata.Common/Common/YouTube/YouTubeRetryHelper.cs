using System;
using Microsoft.Extensions.Hosting;
using System.Threading;
using YouRata.Common.Milestone;
using static YouRata.Common.Proto.MilestoneActionIntelligence.Types;

namespace YouRata.Common.YouTube;

public static class YouTubeRetryHelper
{
    public static void RetryCommand(YouTubeSyncActionIntelligence intelligence, int quotaCost, Action command, TimeSpan minRetry, TimeSpan maxRetry, Action<string> logger)
    {
        int retryCount = 0;
        while (retryCount < 3)
        {
            try
            {
                {
                    intelligence.CalculatedQueriesPerDayRemaining -= quotaCost;
                    command.Invoke();
                }
                break;
            }
            catch (Exception ex)
            {
                retryCount++;
                logger.Invoke($"YouTube API: {ex.Message}");
                if (retryCount > 1)
                {
                    throw new MilestoneException("YouTube API failure", ex);
                }
            }
            TimeSpan backOff = APIBackoffHelper.GetRandomBackoff(minRetry, maxRetry);
            Thread.Sleep(backOff);
        }
    }

    public static T? RetryCommand<T>(YouTubeSyncActionIntelligence intelligence, int quotaCost, Func<T> command, TimeSpan minRetry, TimeSpan maxRetry, Action<string> logger)
    {
        int retryCount = 0;
        T? returnValue = default(T?);
        while (retryCount < 3)
        {
            try
            {
                {
                    intelligence.CalculatedQueriesPerDayRemaining -= quotaCost;
                    returnValue = command.Invoke();
                }
                break;
            }
            catch (Exception ex)
            {
                retryCount++;
                logger.Invoke($"YouTube API: {ex.Message}");
                if (retryCount > 1)
                {
                    throw new MilestoneException("YouTube API failure", ex);
                }
            }
            TimeSpan backOff = APIBackoffHelper.GetRandomBackoff(minRetry, maxRetry);
            Thread.Sleep(backOff);
        }
        return returnValue;
    }
}
