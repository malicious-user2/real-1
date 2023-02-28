using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Octokit;
using Sodium;
using YouRata.Common.Milestone;
using YouRata.Common.Proto;

namespace YouRata.Common.GitHub;

public static class GitHubAPIClient
{
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
        if (environment.RateLimitCoreRemaining < 3)
        {
            return false;
        }
        return true;
    }

    public static bool DeleteSecret(GitHubActionEnvironment environment, string secretName, Action<string> logger)
    {
        if (!HasRemainingCalls(environment)) return false;
        GitHubClient ghClient = new GitHubClient(GitHubConstants.ProductHeader)
        {
            Credentials = new Credentials(environment.ApiToken, AuthenticationType.Bearer)
        };
        ghClient.SetRequestTimeout(GitHubConstants.RequestTimeout);
        IApiConnection apiCon = new ApiConnection(ghClient.Connection);

        RepositorySecretsClient secClient = new RepositorySecretsClient(apiCon);
        string[] repository = environment.EnvGitHubRepository.Split("/");
        Action deleteSecret = (() =>
        {
            try
            {
                secClient.Delete(repository[0], repository[1], secretName).Wait();
            }
            catch (AggregateException e)
            {
                logger(e.Message);
                if (e.InnerException != null && e.InnerException is not NotFoundException)
                {
                    throw new MilestoneException("GitHub API failure", e);
                }
            }
        });
        GitHubRetryHelper.RetryCommand(environment, deleteSecret, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(5), logger);
        return true;
    }

    public static bool CreateOrUpdateSecret(GitHubActionEnvironment environment, string secretName, string secretValue, Action<string> logger)
    {
        if (!HasRemainingCalls(environment)) return false;
        GitHubClient ghClient = new GitHubClient(GitHubConstants.ProductHeader)
        {
            Credentials = new Credentials(environment.ApiToken, AuthenticationType.Bearer)
        };
        ghClient.SetRequestTimeout(GitHubConstants.RequestTimeout);
        IApiConnection apiCon = new ApiConnection(ghClient.Connection);

        RepositorySecretsClient secClient = new RepositorySecretsClient(apiCon);
        string[] repository = environment.EnvGitHubRepository.Split("/");

        SecretsPublicKey? publicKey = default;

        Func<SecretsPublicKey> getPublicKey = (() =>
        {
            return secClient.GetPublicKey(repository[0], repository[1]).Result;
        });
        publicKey = GitHubRetryHelper.RetryCommand(environment, getPublicKey, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(5), logger);
        if (publicKey == null) throw new MilestoneException("Could not get GitHub repository public key to create secret");
        UpsertRepositorySecret secret = CreateSecret(secretValue, publicKey);
        secClient.CreateOrUpdate(repository[0], repository[1], secretName, secret).Wait();
        Action createOrUpdateSecret = (() =>
        {
            secClient.CreateOrUpdate(repository[0], repository[1], secretName, secret).Wait();
        });
        GitHubRetryHelper.RetryCommand(environment, createOrUpdateSecret, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(5), logger);
        return true;
    }
}
