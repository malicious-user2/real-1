using System;
using YouRata.Common;
using YouRata.Common.GitHub;
using YouRata.Common.YouTube;

namespace YouRata.YouTubeSync.Workflow;

internal class YouTubeSyncWorkflow
{
    private readonly string _workspace;
    private readonly string _tokenResponse;
    private readonly string _clientId;
    private readonly string _clientSecret;
    private readonly string _apiKey;

#pragma warning disable CS8601
#pragma warning disable CS8618
    public YouTubeSyncWorkflow()
    {
        _workspace = Environment
            .GetEnvironmentVariable(GitHubConstants.GitHubWorkspaceVariable);
        _tokenResponse = Environment
            .GetEnvironmentVariable(YouRataConstants.StoredTokenResponseVariable);
        _clientId = Environment
            .GetEnvironmentVariable(YouTubeConstants.ProjectClientIdVariable);
        _clientSecret = Environment
            .GetEnvironmentVariable(YouTubeConstants.ProjectClientSecretsVariable);
        _apiKey = Environment
            .GetEnvironmentVariable(YouTubeConstants.ProjectApiKeyVariable);
    }
#pragma warning restore CS8601
#pragma warning restore CS8618

    public string Workspace => _workspace;

    public string StoredTokenResponse => _tokenResponse;

    public string ProjectClientId => _clientId;

    public string ProjectClientSecret => _clientSecret;

    public string ProjectApiKey => _apiKey;
}
