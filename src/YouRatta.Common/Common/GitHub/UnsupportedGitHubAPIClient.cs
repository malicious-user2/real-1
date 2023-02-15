using System;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Threading;
using Newtonsoft.Json.Linq;
using YouRatta.Common.Proto;
using Newtonsoft.Json;
using System.Text;

namespace YouRatta.Common.GitHub;

public static class UnsupportedGitHubAPIClient
{
    private static void RetryCommand(GitHubActionEnvironment environment, Action command, TimeSpan minRetry, TimeSpan maxRetry, Action<string> logger)
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
                    throw;
                }
            }
            TimeSpan backOff = APIBackoffHelper.GetRandomBackoff(minRetry, maxRetry);
            Thread.Sleep(backOff);
        }
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

    public static bool CreateVariable(GitHubActionEnvironment environment, string variableName, string variableValue, Action<string> logger)
    {
        if (!HasRemainingCalls(environment)) return false;
        if (environment.EnvGitHubRepository == null) return false;
        if (environment.EnvGitHubRepository.Split("/").Length != 2) return false;
        HttpResponseMessage? response = null;
        Action createVariable = (() =>
        {
            using (HttpClient client = new HttpClient())
            {
                client.Timeout = GitHubConstants.RequestTimeout;
                client.BaseAddress = new Uri($"{environment.EnvGitHubApiUrl}/");
                client.DefaultRequestHeaders.Add("User-Agent", GitHubConstants.ProductHeaderName);
                client.DefaultRequestHeaders.Add("Accept", "application/json");
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", environment.ApiToken);
                RepositoryVariable variable = new RepositoryVariable();
                variable.Name = variableName;
                variable.Value = variableValue;
                string contentData = JsonConvert.SerializeObject(variable);
                HttpContent content = new StringContent(contentData);
                response = client.PostAsync($"repos/{environment.EnvGitHubRepository}/actions/variables", content).Result;
            }
        });
        RetryCommand(environment, createVariable, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(5), logger);
        if (response != null && response.IsSuccessStatusCode) return true;
        return false;
    }

    public class RepositoryVariable
    {
        [JsonProperty(PropertyName = "name")]
        public string? Name { get; set; }

        [JsonProperty(PropertyName = "value")]
        public string? Value { get; set; }
    }
}
