using System;
using YouRatta.Common.GitHub;

namespace YouRatta.ConflictMonitor.Workflow;

internal class ConflictMonitorWorkflow
{
    private readonly string _actionToken;
    private readonly string _clientSecrets;

    public ConflictMonitorWorkflow()
    {
        _actionToken = Environment
            .GetEnvironmentVariable(GitHubConstants.ActionTokenVariable);
        _clientSecrets = Environment
            .GetEnvironmentVariable(GitHubConstants.ClientSecretsVariable);
    }

    public string GithubToken => _actionToken;

    public string YouTubeClientSecrets => _clientSecrets;
}
