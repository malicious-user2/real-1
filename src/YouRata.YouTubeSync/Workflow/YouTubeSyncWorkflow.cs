using System;
using YouRata.Common;
using YouRata.Common.YouTube;

namespace YouRata.YouTubeSync.Workflow;

internal class YouTubeSyncWorkflow
{
    private readonly bool _initialSetupComplete;

#pragma warning disable CS8601
#pragma warning disable CS8618
    public YouTubeSyncWorkflow()
    {
        bool.TryParse(Environment
            .GetEnvironmentVariable(YouRataConstants.InitialSetupCompleteVariable),
            out _initialSetupComplete);
    }
#pragma warning restore CS8601
#pragma warning restore CS8618

    public bool InitialSetupComplete => _initialSetupComplete;
}
