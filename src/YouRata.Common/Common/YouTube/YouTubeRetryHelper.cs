using System;
using Microsoft.Extensions.Hosting;
using System.Threading;
using YouRata.Common.Milestone;
using static YouRata.Common.Proto.MilestoneActionIntelligence.Types;
using Google;
using System.Net;

namespace YouRata.Common.YouTube;

public static class YouTubeRetryHelper
{
    public static void RetryCommand(YouTubeSyncActionIntelligence intelligence, int quotaCost, Action command, TimeSpan minRetry, TimeSpan maxRetry, Action<string> logger)
    {
        RetryCommand(intelligence, quotaCost, command, minRetry, maxRetry, logger, null, out _);
    }

    public static void RetryCommand(YouTubeSyncActionIntelligence intelligence, int quotaCost, Action command, TimeSpan minRetry, TimeSpan maxRetry, Action<string> logger, HttpStatusCode? trapStatus, out bool trapped)
    {
        trapped = false;
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
            catch (GoogleApiException ex) when (trapStatus != null)
            {
                if (ex.HttpStatusCode == trapStatus)
                {
                    logger.Invoke($"YouTube API: {ex.Message}");
                    trapped = true;
                    break;
                }
                else
                {
                    retryCount++;
                    if (retryCount > 1)
                    {
                        throw new MilestoneException("YouTube API failure", ex);
                    }
                }
            }
            catch (Exception)
            {
                retryCount++;
                if (retryCount > 2)
                {
                    throw;
                }
            }
            TimeSpan backOff = APIBackoffHelper.GetRandomBackoff(minRetry, maxRetry);
            Thread.Sleep(backOff);
        }
    }

    public static T? RetryCommand<T>(YouTubeSyncActionIntelligence intelligence, int quotaCost, Func<T> command, TimeSpan minRetry, TimeSpan maxRetry, Action<string> logger)
    {
        return RetryCommand(intelligence, quotaCost, command, minRetry, maxRetry, logger, null, out _);
    }

    public static T? RetryCommand<T>(YouTubeSyncActionIntelligence intelligence, int quotaCost, Func<T> command, TimeSpan minRetry, TimeSpan maxRetry, Action<string> logger, HttpStatusCode? trapStatus, out bool trapped)
    {
        trapped = false;
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
            catch (GoogleApiException ex) when (trapStatus != null)
            {
                if (ex.HttpStatusCode == trapStatus)
                {
                    logger.Invoke($"YouTube API: {ex.Message}");
                    trapped = true;
                    break;
                }
                else
                {
                    retryCount++;
                    if (retryCount > 1)
                    {
                        throw new MilestoneException("YouTube API failure", ex);
                    }
                }
            }
            catch (Exception)
            {
                retryCount++;
                if (retryCount > 2)
                {
                    throw;
                }
            }
            TimeSpan backOff = APIBackoffHelper.GetRandomBackoff(minRetry, maxRetry);
            Thread.Sleep(backOff);
        }
        return returnValue;
    }
}
