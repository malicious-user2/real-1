using System;
using YouRatta.Common;
using YouRatta.Common.GitHub;

namespace YouRatta.ConflictMonitor.Workflow;

internal class ConflictMonitorWorkflow
{
    private readonly string _actionToken;
    private readonly string _apiToken;
    private readonly string _clientSecrets;

#pragma warning disable CS8601
#pragma warning disable CS8618
    public ConflictMonitorWorkflow()
    {
        _actionToken = Environment
            .GetEnvironmentVariable(GitHubConstants.ActionTokenVariable);
        _apiToken = Environment
            .GetEnvironmentVariable(GitHubConstants.ApiTokenVariable);
        _clientSecrets = Environment
            .GetEnvironmentVariable(YouRattaConstants.StoredClientSecretsVariable);
    }
#pragma warning restore CS8601
#pragma warning restore CS8618

    public string GitHubToken => _actionToken;

    public string ApiToken => _apiToken;

    public string YouTubeClientSecrets => _clientSecrets;
}
