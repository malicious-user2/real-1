using System;
using YouRata.Common.GitHub;
using YouRata.Common;
using YouRata.Common.YouTube;

namespace YouRata.InitialSetup.Workflow;

internal class InitialSetupWorkflow
{
    private readonly string _tokenResponse;
    private readonly string _clientId;
    private readonly string _clientSecret;
    private readonly string _apiKey;
    private readonly string _redirectCode;

#pragma warning disable CS8601
#pragma warning disable CS8618
    public InitialSetupWorkflow()
    {
        _redirectCode = Environment
            .GetEnvironmentVariable(YouTubeConstants.RedirectCodeVariable);
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

    public string RedirectCode => _redirectCode;

    public string StoredTokenResponse => _tokenResponse;

    public string ProjectClientId => _clientId;

    public string ProjectClientSecret => _clientSecret;

    public string ProjectApiKey => _apiKey;
}
