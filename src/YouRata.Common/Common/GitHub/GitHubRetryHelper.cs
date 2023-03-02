using System;
using System.Threading;
using YouRata.Common.Milestone;
using YouRata.Common.Proto;

namespace YouRata.Common.GitHub;

public static class GitHubRetryHelper
{
    public static void RetryCommand(GitHubActionEnvironment environment, Action command, Action<string> logger)
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
            catch (Exception)
            {
                retryCount++;
                if (retryCount > 1)
                {
                    throw;
                }
            }
            TimeSpan backOff = APIBackoffHelper.GetRandomBackoff(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(5));
            Thread.Sleep(backOff);
        }
    }

    public static T? RetryCommand<T>(GitHubActionEnvironment environment, Func<T> command, Action<string> logger)
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
            catch (Exception)
            {
                retryCount++;
                if (retryCount > 1)
                {
                    throw;
                }
            }
            TimeSpan backOff = APIBackoffHelper.GetRandomBackoff(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(5));
            Thread.Sleep(backOff);
        }
        return returnValue;
    }
}
