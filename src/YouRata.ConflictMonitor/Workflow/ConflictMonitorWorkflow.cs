using System;
using YouRata.Common;
using YouRata.Common.GitHub;

namespace YouRata.ConflictMonitor.Workflow;

internal class ConflictMonitorWorkflow
{
    private readonly string _actionToken;
    private readonly string _apiToken;
    private readonly string _tokenResponse;

#pragma warning disable CS8601
#pragma warning disable CS8618
    public ConflictMonitorWorkflow()
    {
        _actionToken = Environment
            .GetEnvironmentVariable(GitHubConstants.ActionTokenVariable);
        _apiToken = Environment
            .GetEnvironmentVariable(GitHubConstants.ApiTokenVariable);
        _tokenResponse = Environment
            .GetEnvironmentVariable(YouRataConstants.StoredTokenResponseVariable);
        InitialSetupComplete = false;
    }
#pragma warning restore CS8601
#pragma warning restore CS8618

    public string GitHubToken => _actionToken;

    public string ApiToken => _apiToken;

    public string StoredTokenResponse => _tokenResponse;

    public bool InitialSetupComplete
    {
        set => GitHubWorkflowHelper
            .PushVariable(YouRataConstants.InitialSetupCompleteVariable, value.ToString());
    }
}
