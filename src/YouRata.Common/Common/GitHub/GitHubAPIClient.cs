using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
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

    private static IApiConnection GetApiConnection(string token)
    {
        GitHubClient ghClient = new GitHubClient(GitHubConstants.ProductHeader)
        {
            Credentials = new Credentials(token, AuthenticationType.Bearer)
        };
        ghClient.SetRequestTimeout(GitHubConstants.RequestTimeout);
        return new ApiConnection(ghClient.Connection);
    }

    public static bool DeleteSecret(GitHubActionEnvironment environment, string secretName, Action<string> logger)
    {
        if (!HasRemainingCalls(environment)) throw new MilestoneException("GitHub API rate limit exceeded");
        IApiConnection apiCon = GetApiConnection(environment.ApiToken);

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
        GitHubRetryHelper.RetryCommand(environment, deleteSecret, logger);
        return true;
    }

    public static bool CreateContentFile(GitHubActionEnvironment environment, string message, string content, string path, Action<string> logger)
    {
        if (!HasRemainingCalls(environment)) throw new MilestoneException("GitHub API rate limit exceeded");
        IApiConnection apiCon = GetApiConnection(environment.ApiToken);

        string[] repository = environment.EnvGitHubRepository.Split("/");
        CreateFileRequest createFileRequest = new CreateFileRequest(message, content, GitHubConstants.ErrataBranch);
        RepositoryContentsClient conClient = new RepositoryContentsClient(apiCon);
        Action createFile = (() =>
        {
            conClient.CreateFile(repository[0], repository[1], path, createFileRequest).Wait();
        });
        GitHubRetryHelper.RetryCommand(environment, createFile, logger);
        return true;
    }

    public static bool UpdateContentFile(GitHubActionEnvironment environment, string message, string content, string path, Action<string> logger)
    {
        if (!HasRemainingCalls(environment)) throw new MilestoneException("GitHub API rate limit exceeded");
        IApiConnection apiCon = GetApiConnection(environment.ApiToken);

        string[] repository = environment.EnvGitHubRepository.Split("/");
        RepositoryContentsClient conClient = new RepositoryContentsClient(apiCon);
        IReadOnlyList<RepositoryContent>? foundContent = default;
        Action getContents = (() =>
        {
            foundContent = conClient.GetAllContentsByRef(repository[0], repository[1], path, GitHubConstants.ErrataBranch).Result;
        });
        GitHubRetryHelper.RetryCommand(environment, getContents, logger);
        if (foundContent == null || foundContent.Count == 0) throw new MilestoneException($"Could not find any content at {path} to update in GitHub");
        RepositoryContent oldContent = foundContent.First();
        UpdateFileRequest updateFileRequest = new UpdateFileRequest(message, content, oldContent.Sha, GitHubConstants.ErrataBranch);
        Action updateFile = (() =>
        {
            conClient.UpdateFile(repository[0], repository[1], path, updateFileRequest).Wait();
        });
        GitHubRetryHelper.RetryCommand(environment, updateFile, logger);
        return true;
    }

    public static bool CreateOrUpdateSecret(GitHubActionEnvironment environment, string secretName, string secretValue, Action<string> logger)
    {
        if (!HasRemainingCalls(environment)) throw new MilestoneException("GitHub API rate limit exceeded");
        IApiConnection apiCon = GetApiConnection(environment.ApiToken);

        RepositorySecretsClient secClient = new RepositorySecretsClient(apiCon);
        string[] repository = environment.EnvGitHubRepository.Split("/");

        SecretsPublicKey? publicKey = default;

        Func<SecretsPublicKey> getPublicKey = (() =>
        {
            return secClient.GetPublicKey(repository[0], repository[1]).Result;
        });
        publicKey = GitHubRetryHelper.RetryCommand(environment, getPublicKey, logger);
        if (publicKey == null) throw new MilestoneException("Could not get GitHub repository public key to create secret");
        UpsertRepositorySecret secret = CreateSecret(secretValue, publicKey);
        secClient.CreateOrUpdate(repository[0], repository[1], secretName, secret).Wait();
        Action createOrUpdateSecret = (() =>
        {
            secClient.CreateOrUpdate(repository[0], repository[1], secretName, secret).Wait();
        });
        GitHubRetryHelper.RetryCommand(environment, createOrUpdateSecret, logger);
        return true;
    }
}