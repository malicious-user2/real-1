using System;
using YouRatta.Common.YouTube;

namespace YouRatta.InitialSetup.Workflow;

internal class InitialSetupWorkflow
{
    private readonly string _clientId;
    private readonly string _clientSecret;
    private readonly string _redirectCode;

#pragma warning disable CS8601
#pragma warning disable CS8618
    public InitialSetupWorkflow()
    {
        _clientId = Environment
            .GetEnvironmentVariable(YouTubeConstants.ProjectClientIdVariable);
        _clientSecret = Environment
            .GetEnvironmentVariable(YouTubeConstants.ProjectClientSecretsVariable);
        _redirectCode = Environment
            .GetEnvironmentVariable(YouTubeConstants.RedirectCodeVariable);
    }
#pragma warning restore CS8601
#pragma warning restore CS8618

    public string ClientId => _clientId;

    public string ClientSecret => _clientSecret;

    public string RedirectCode => _redirectCode;
}
