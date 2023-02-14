using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Octokit;
using Sodium;
using YouRatta.Common.Proto;

namespace YouRatta.Common.GitHub;

public static class GitHubAPIClient
{
    private static void RetryCommand(GitHubActionEnvironment environment, Action command, TimeSpan minRetry, TimeSpan maxRetry, Action<string> logger)
    {
        int retryCount = 0;
        while (retryCount < 3)
        {
            try
            {
                environment.RateLimitCoreRemaining--;
                command.Invoke();
                break;
            }
            catch (Exception ex)
            {
                retryCount++;
                logger.Invoke(ex.Message);
                if (retryCount > 1)
                {
                    throw;
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
                environment.RateLimitCoreRemaining--;
                returnValue = command.Invoke();
                break;
            }
            catch (Exception ex)
            {
                retryCount++;
                logger.Invoke(ex.Message);
                if (retryCount > 1)
                {
                    throw;
                }
            }
            TimeSpan backOff = APIBackoffHelper.GetRandomBackoff(minRetry, maxRetry);
            Thread.Sleep(backOff);
        }
        return returnValue;
    }

    private static UpsertRepositorySecret CreateSecret(string secretValue, SecretsPublicKey key)
    {
        byte[] secretBytes = Encoding.UTF8.GetBytes(secretValue);
        byte[] publicKey = Convert.FromBase64String(key.Key);
        byte[] sealedPublicKeyBox = SealedPublicKeyBox.Create(secretBytes, publicKey);

        UpsertRepositorySecret upsertValue = new UpsertRepositorySecret
        {
            EncryptedValue = Convert.ToBase64String(sealedPublicKeyBox),
            KeyId = key.KeyId
        };

        return upsertValue;
    }

    public static bool HasRemainingCalls(GitHubActionEnvironment environment)
    {
        if (environment.RateLimitCoreRemaining < 100 && environment.RateLimitCoreRemaining > 0)
        {
            Console.WriteLine($"WARNING: Only {environment.RateLimitCoreRemaining} GitHub API calls remaining");
        }
        if (environment.RateLimitCoreRemaining < 2)
        {
            return false;
        }
        return true;
    }

    public static bool DeleteSecret(GitHubActionEnvironment environment, string secretName, Action<string> logger)
    {
        if (!HasRemainingCalls(environment)) return false;
        Action deleteSecret = (() =>
        {
            GitHubClient ghClient = new GitHubClient(GitHubConstants.ProductHeader)
            {
                Credentials = new Credentials(environment.ApiToken, AuthenticationType.Bearer)
            };
            ghClient.SetRequestTimeout(GitHubConstants.RequestTimeout);
            IApiConnection apiCon = new ApiConnection(ghClient.Connection);

            RepositorySecretsClient secClient = new RepositorySecretsClient(apiCon);
            string[] repository = environment.EnvGitHubRepository.Split("/");
            secClient.Delete(repository[0], repository[1], secretName).Wait();
        });
        RetryCommand(environment, deleteSecret, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(5), logger);
        return true;
    }

    public static bool CreateOrUpdateSecret(GitHubActionEnvironment environment, string secretName, string secretValue, Action<string> logger)
    {
        if (!HasRemainingCalls(environment)) return false;
        Action updateSecret = (() =>
        {
            GitHubClient ghClient = new GitHubClient(GitHubConstants.ProductHeader)
            {
                Credentials = new Credentials(environment.ApiToken, AuthenticationType.Bearer)
            };
            ghClient.SetRequestTimeout(GitHubConstants.RequestTimeout);
            IApiConnection apiCon = new ApiConnection(ghClient.Connection);

            RepositorySecretsClient secClient = new RepositorySecretsClient(apiCon);
            string[] repository = environment.EnvGitHubRepository.Split("/");


            Console.WriteLine(ghClient.RateLimit.GetRateLimits().Result.Resources.Core.Remaining);



            SecretsPublicKey publicKey = secClient.GetPublicKey(repository[0], repository[1]).Result;




            Console.WriteLine(ghClient.RateLimit.GetRateLimits().Result.Resources.Core.Remaining);



            UpsertRepositorySecret secret = CreateSecret(secretValue, publicKey);
            secClient.CreateOrUpdate(repository[0], repository[1], secretName, secret).Wait();
        });
        RetryCommand(environment, updateSecret, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(5), logger);
        return true;
    }
}
