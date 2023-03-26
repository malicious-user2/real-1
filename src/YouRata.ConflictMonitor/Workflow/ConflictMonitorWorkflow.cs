// Copyright (c) 2023 battleship-systems.
// Licensed under the MIT license.

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
            .GetEnvironmentVariable(YouRataConstants.ActionTokenVariable);
        _apiToken = Environment
            .GetEnvironmentVariable(YouRataConstants.ApiTokenVariable);
        _tokenResponse = Environment
            .GetEnvironmentVariable(YouRataConstants.StoredTokenResponseVariable);
        InitialSetupComplete = false;
    }
#pragma warning restore CS8601
#pragma warning restore CS8618

    public string ApiToken => _apiToken;

    public string GitHubToken => _actionToken;

    public bool InitialSetupComplete
    {
        set => GitHubWorkflowHelper
            .PushVariable(YouRataConstants.InitialSetupCompleteVariable, value.ToString());
    }

    public string StoredTokenResponse => _tokenResponse;
}
