// Copyright (c) 2023 battleship-systems.
// Licensed under the MIT license.

using System;
using System.Threading;
using YouRata.Common.Milestone;
using YouRata.Common.Proto;

namespace YouRata.Common.GitHub;

public static class GitHubRetryHelper
{
    public static void RetryCommand(GitHubActionEnvironment environment, Action command, Action<string> logger)
    {
        RetryCommand(environment, command, logger, null, out _);
    }

    public static void RetryCommand(GitHubActionEnvironment environment, Action command, Action<string> logger, Type? trapInnerException,
        out bool trapped)
    {
        trapped = false;
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
            catch (Exception ex) when (trapInnerException != null)
            {
                if (ex.InnerException != null && ex.InnerException.GetType() == trapInnerException)
                {
                    logger.Invoke($"GitHub API: {ex.Message}");
                    trapped = true;
                    break;
                }

                retryCount++;
                if (retryCount > 1)
                {
                    throw new MilestoneException("GitHub API failure", ex);
                }
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
        return RetryCommand(environment, command, logger, null, out _);
    }

    public static T? RetryCommand<T>(GitHubActionEnvironment environment, Func<T> command, Action<string> logger, Type? trapInnerException,
        out bool trapped)
    {
        trapped = false;
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
            catch (Exception ex) when (trapInnerException != null)
            {
                if (ex.InnerException != null && ex.InnerException.GetType() == trapInnerException)
                {
                    logger.Invoke($"GitHub API: {ex.Message}");
                    trapped = true;
                    break;
                }

                retryCount++;
                if (retryCount > 1)
                {
                    throw new MilestoneException("GitHub API failure", ex);
                }
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
