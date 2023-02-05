using System;

namespace YouRatta.ConflictMonitor.Workflow;

internal class ConflictMonitorWorkflow
{
    private readonly string _actionToken;

    public ConflictMonitorWorkflow()
    {
        _actionToken = Environment.GetEnvironmentVariable("ACTION_TOKEN");
    }

    public string GithubToken => _actionToken;
}
