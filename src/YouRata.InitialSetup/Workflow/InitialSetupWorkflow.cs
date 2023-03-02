using System;
using YouRata.Common.GitHub;
using YouRata.Common;
using YouRata.Common.YouTube;

namespace YouRata.InitialSetup.Workflow;

internal class InitialSetupWorkflow
{
    private readonly string _redirectCode;

#pragma warning disable CS8601
#pragma warning disable CS8618
    public InitialSetupWorkflow()
    {
        _redirectCode = Environment
            .GetEnvironmentVariable(YouTubeConstants.RedirectCodeVariable);
    }
#pragma warning restore CS8601
#pragma warning restore CS8618

    public string RedirectCode => _redirectCode;
}
