using System;
using YouRatta.Common.GitHub;

namespace YouRatta.ConflictMonitor.Workflow;

internal class ConflictMonitorWorkflow
{
    private readonly string _actionToken;
    private readonly string _clientSecrets;

#pragma warning disable CS8601
#pragma warning disable CS8618
    public ConflictMonitorWorkflow()
    {
        _actionToken = Environment
            .GetEnvironmentVariable(GitHubConstants.ActionTokenVariable);
        _clientSecrets = Environment
            .GetEnvironmentVariable(GitHubConstants.ClientSecretsVariable);
    }
#pragma warning restore CS8601
#pragma warning restore CS8618

    public string GitHubToken => _actionToken;

    public string YouTubeClientSecrets => _clientSecrets;
}
