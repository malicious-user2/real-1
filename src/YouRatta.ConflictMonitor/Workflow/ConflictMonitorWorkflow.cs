using System;
using YouRatta.Common;
using YouRatta.Common.GitHub;
using YouRatta.Common.YouTube;

namespace YouRatta.ConflictMonitor.Workflow;

internal class ConflictMonitorWorkflow
{
    private readonly string _actionToken;
    private readonly string _apiToken;
    private readonly string _tokenResponse;
    private readonly string _clientId;
    private readonly string _clientSecret;
    private readonly string _apiKey;

#pragma warning disable CS8601
#pragma warning disable CS8618
    public ConflictMonitorWorkflow()
    {
        _actionToken = Environment
            .GetEnvironmentVariable(GitHubConstants.ActionTokenVariable);
        _apiToken = Environment
            .GetEnvironmentVariable(GitHubConstants.ApiTokenVariable);
        _tokenResponse = Environment
            .GetEnvironmentVariable(YouRattaConstants.StoredTokenResponseVariable);
        _clientId = Environment
            .GetEnvironmentVariable(YouTubeConstants.ProjectClientIdVariable);
        _clientSecret = Environment
            .GetEnvironmentVariable(YouTubeConstants.ProjectClientSecretsVariable);
        _apiKey = Environment
            .GetEnvironmentVariable(YouTubeConstants.ProjectApiKeyVariable);
        InitialSetupComplete = false;
    }
#pragma warning restore CS8601
#pragma warning restore CS8618

    public string GitHubToken => _actionToken;

    public string ApiToken => _apiToken;

    public string StoredTokenResponse => _tokenResponse;

    public string ProjectClientId => _clientId;

    public string ProjectClientSecret => _clientSecret;

    public string ProjectApiKey => _apiKey;

    public bool InitialSetupComplete
    {
        set => GitHubWorkflowHelper
            .PushVariable(YouRattaConstants.InitialSetupCompleteVariable, value.ToString());
    }

}
