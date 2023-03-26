// Copyright (c) 2023 battleship-systems.
// Licensed under the MIT license.

using System;
using YouRata.Common;

namespace YouRata.YouTubeSync.Workflow;

internal class YouTubeSyncWorkflow
{
    private readonly string _apiKey;
    private readonly string _clientId;
    private readonly string _clientSecret;
    private readonly string _tokenResponse;
    private readonly string _workspace;

#pragma warning disable CS8601
#pragma warning disable CS8618
    public YouTubeSyncWorkflow()
    {
        _apiKey = Environment
            .GetEnvironmentVariable(YouRataConstants.ProjectApiKeyVariable);
        _clientId = Environment
            .GetEnvironmentVariable(YouRataConstants.ProjectClientIdVariable);
        _clientSecret = Environment
            .GetEnvironmentVariable(YouRataConstants.ProjectClientSecretsVariable);
        _tokenResponse = Environment
            .GetEnvironmentVariable(YouRataConstants.StoredTokenResponseVariable);
        _workspace = Environment
            .GetEnvironmentVariable(YouRataConstants.GitHubWorkspaceVariable);
    }
#pragma warning restore CS8601
#pragma warning restore CS8618

    public string ProjectApiKey => _apiKey;

    public string ProjectClientId => _clientId;

    public string ProjectClientSecret => _clientSecret;

    public string StoredTokenResponse => _tokenResponse;

    public string Workspace => _workspace;
}
