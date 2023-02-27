using System;
using System.Threading;
using YouRata.Common.Milestone;
using YouRata.Common.Proto;

namespace YouRata.Common.GitHub;

public static class GitHubRetryHelper
{
    public static void RetryCommand(GitHubActionEnvironment environment, Action command, TimeSpan minRetry, TimeSpan maxRetry, Action<string> logger)
    {
        int retryCount = 0;
        while (retryCount < 3)
        {
            try
            {
                {
                    environment.RateLimitCoreRemaining--;
                    command.Invoke();
                }
                break;
            }
            catch (Exception ex)
            {
                retryCount++;
                logger.Invoke(ex.Message);
                if (retryCount > 1)
                {
                    throw new MilestoneException("GitHub API failure", ex);
                }
            }
            TimeSpan backOff = APIBackoffHelper.GetRandomBackoff(minRetry, maxRetry);
            Thread.Sleep(backOff);
        }
    }

    public static T? RetryCommand<T>(GitHubActionEnvironment environment, Func<T> command, TimeSpan minRetry, TimeSpan maxRetry, Action<string> logger)
    {
        int retryCount = 0;
        T? returnValue = default(T?);
        while (retryCount < 3)
        {
            try
            {
                {
                    environment.RateLimitCoreRemaining--;
                    returnValue = command.Invoke();
                }
                break;
            }
            catch (Exception ex)
            {
                retryCount++;
                logger.Invoke(ex.Message);
                if (retryCount > 1)
                {
                    throw new MilestoneException("GitHub API failure", ex);
                }
            }
            TimeSpan backOff = APIBackoffHelper.GetRandomBackoff(minRetry, maxRetry);
            Thread.Sleep(backOff);
        }
        return returnValue;
    }

    public static T? ForceRetryCommand<T>(Func<T> command, TimeSpan minRetry, TimeSpan maxRetry, Action<string> logger)
    {
        int retryCount = 0;
        T? returnValue = default(T?);
        while (retryCount < 10)
        {
            try
            {
                returnValue = command.Invoke();
                break;
            }
            catch (Exception ex)
            {
                retryCount++;
                logger.Invoke(ex.Message);
                if (retryCount > 9)
                {
                    throw new MilestoneException("GitHub API failure", ex);
                }
            }
            TimeSpan backOff = APIBackoffHelper.GetRandomBackoff(minRetry, maxRetry);
            Thread.Sleep(backOff);
        }
        return returnValue;
    }
}
