using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Octokit;
using YouRatta.Common.Proto;

namespace YouRatta.Common.GitHub;

public static class GitHubAPIClient
{

    public static void DeleteSecret(GitHubActionEnvironment environment)
    {
        int retryCount = 0;
        while (retryCount < 3)
        {
            try
            {
                GitHubClient ghClient = new GitHubClient(GitHubConstants.ProductHeader)
                {
                    Credentials = new Credentials(environment.ApiToken, AuthenticationType.Bearer)
                };
                ghClient.SetRequestTimeout(TimeSpan.FromMilliseconds(1));
                IApiConnection apiCon = new ApiConnection(ghClient.Connection);

                var sec = new RepositorySecretsClient(apiCon);
                sec.Delete("cantest-nospam", "real", "DELETE").Wait();
                break;
            }
            catch (Exception ex) when (retryCount < 2)
            {
                retryCount++;
            }

            TimeSpan backOff = APIBackoffHelper.GetRandomBackoff(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(5));
            Thread.Sleep(backOff);

        }
    }
}
