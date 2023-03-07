using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Octokit;
using YouRata.Common;
using YouRata.Common.ActionReport;
using YouRata.Common.Configurations;
using YouRata.Common.GitHub;
using YouRata.Common.Proto;
using YouRata.ConflictMonitor.ActionReport;
using YouRata.ConflictMonitor.MilestoneData;
using YouRata.ConflictMonitor.Workflow;
using static YouRata.Common.Proto.MilestoneActionIntelligence.Types;

namespace YouRata.ConflictMonitor.MilestoneCall;

internal class CallHandler
{
    private readonly List<string> _logBuilder;

    internal CallHandler()
    {
        _logBuilder = new List<string>();
    }

    internal GitHubActionEnvironment GetGithubActionEnvironment(YouRataConfiguration appConfig, GitHubEnvironment environment, ConflictMonitorWorkflow workflow)
    {
        GitHubActionEnvironment actionEnvironment = environment.GetActionEnvironment();
        if (!appConfig.ActionCutOuts.DisableConflictMonitorGitHubOperations && workflow.GitHubToken != null && workflow.ApiToken != null)
        {
            ResourceRateLimit? ghRateLimit = default;
            Func<ResourceRateLimit> getResourceRateLimit = (() =>
            {
                GitHubClient ghClient = new GitHubClient(GitHubConstants.ProductHeader);
                ghClient.Credentials = new Credentials(workflow.GitHubToken, AuthenticationType.Bearer);
                return ghClient.RateLimit.GetRateLimits().Result.Resources;
            });
            ghRateLimit = GitHubRetryHelper.RetryCommand<ResourceRateLimit>(actionEnvironment.OverrideRateLimit(), getResourceRateLimit, AppendLog);
            if (ghRateLimit != null)
            {
                actionEnvironment.RateLimitCoreRemaining = ghRateLimit.Core.Remaining;
                actionEnvironment.RateLimitCoreLimit = ghRateLimit.Core.Limit;
                actionEnvironment.RateLimitCoreReset = ghRateLimit.Core.Reset.ToUnixTimeSeconds();
            }
            actionEnvironment.GitHubToken = workflow.GitHubToken;
            actionEnvironment.ApiToken = workflow.ApiToken;
        }
        return actionEnvironment;
    }

    internal string GetStoredTokenResponse(YouRataConfiguration appConfig, ConflictMonitorWorkflow workflow)
    {
        if (!appConfig.ActionCutOuts.DisableStoredTokenResponseDiscovery && workflow.StoredTokenResponse != null)
        {
            return workflow.StoredTokenResponse;
        }
        return string.Empty;
    }

    internal string GetAppClientId(YouRataConfiguration appConfig, ConflictMonitorWorkflow workflow)
    {
        if (workflow.ProjectClientId != null)
        {
            return workflow.ProjectClientId;
        }
        return string.Empty;
    }

    internal string GetAppClientSecret(YouRataConfiguration appConfig, ConflictMonitorWorkflow workflow)
    {
        if (workflow.ProjectClientSecret != null)
        {
            return workflow.ProjectClientSecret;
        }
        return string.Empty;
    }

    internal string GetAppApiKey(YouRataConfiguration appConfig, ConflictMonitorWorkflow workflow)
    {
        if (workflow.ProjectApiKey != null)
        {
            return workflow.ProjectApiKey;
        }
        return string.Empty;
    }

    internal string GetPreviousActionReport(YouRataConfiguration appConfig, PreviousActionReportProvider actionReportProvider)
    {
        if (actionReportProvider.ActionReport != null)
        {
            return JsonConvert.SerializeObject(actionReportProvider.ActionReport, Formatting.None);
        }
        return string.Empty;
    }

    internal MilestoneActionIntelligence GetMilestoneActionIntelligence(YouRataConfiguration appConfig, MilestoneIntelligenceRegistry milestoneIntelligence)
    {
        MilestoneActionIntelligence actionIntelligence = new MilestoneActionIntelligence();
        actionIntelligence.InitialSetup = new InitialSetupActionIntelligence();
        actionIntelligence.InitialSetup.Condition = milestoneIntelligence.InitialSetup.Condition;
        actionIntelligence.InitialSetup.ProcessId = milestoneIntelligence.InitialSetup.ProcessId;
        actionIntelligence.YouTubeSync = new YouTubeSyncActionIntelligence();
        actionIntelligence.YouTubeSync.Condition = milestoneIntelligence.YouTubeSync.Condition;
        actionIntelligence.YouTubeSync.ProcessId = milestoneIntelligence.YouTubeSync.ProcessId;
        actionIntelligence.YouTubeSync.VideosProcessed = milestoneIntelligence.YouTubeSync.VideosProcessed;
        actionIntelligence.YouTubeSync.VideosSkipped = milestoneIntelligence.YouTubeSync.VideosSkipped;
        actionIntelligence.YouTubeSync.CalculatedQueriesPerDayRemaining = milestoneIntelligence.YouTubeSync.CalculatedQueriesPerDayRemaining;
        actionIntelligence.YouTubeSync.LastQueryTime = milestoneIntelligence.YouTubeSync.LastQueryTime;
        actionIntelligence.YouTubeSync.LastVideoPublishTime = milestoneIntelligence.YouTubeSync.LastVideoPublishTime;
        actionIntelligence.ActionReport = new ActionReportActionIntelligence();
        actionIntelligence.ActionReport.Condition = milestoneIntelligence.ActionReport.Condition;
        actionIntelligence.ActionReport.ProcessId = milestoneIntelligence.ActionReport.ProcessId;

        return actionIntelligence;
    }

    internal void UpdateInitialSetupActionIntelligence(YouRataConfiguration appConfig, MilestoneIntelligenceRegistry milestoneIntelligence, InitialSetupActionIntelligence actionIntelligence)
    {
        long updateTime = DateTimeOffset.Now.ToUnixTimeSeconds();
        milestoneIntelligence.InitialSetup.Condition = actionIntelligence.Condition;
        milestoneIntelligence.InitialSetup.ProcessId = actionIntelligence.ProcessId;
        if (milestoneIntelligence.InitialSetup.StartTime == 0)
        {
            milestoneIntelligence.InitialSetup.StartTime = updateTime;
        }
        milestoneIntelligence.InitialSetup.LastUpdate = updateTime;
    }

    internal void UpdateYouTubeSyncActionIntelligence(YouRataConfiguration appConfig, MilestoneIntelligenceRegistry milestoneIntelligence, YouTubeSyncActionIntelligence actionIntelligence)
    {
        long updateTime = DateTimeOffset.Now.ToUnixTimeSeconds();
        milestoneIntelligence.YouTubeSync.Condition = actionIntelligence.Condition;
        milestoneIntelligence.YouTubeSync.ProcessId = actionIntelligence.ProcessId;
        if (milestoneIntelligence.YouTubeSync.StartTime == 0)
        {
            milestoneIntelligence.YouTubeSync.StartTime = updateTime;
        }
        milestoneIntelligence.YouTubeSync.LastUpdate = updateTime;
        milestoneIntelligence.YouTubeSync.VideosProcessed = actionIntelligence.VideosProcessed;
        milestoneIntelligence.YouTubeSync.VideosSkipped = actionIntelligence.VideosSkipped;
        milestoneIntelligence.YouTubeSync.CalculatedQueriesPerDayRemaining = actionIntelligence.CalculatedQueriesPerDayRemaining;
        milestoneIntelligence.YouTubeSync.LastQueryTime = actionIntelligence.LastQueryTime;
        milestoneIntelligence.YouTubeSync.LastVideoPublishTime = actionIntelligence.LastVideoPublishTime;
    }

    internal void UpdateActionReportMilestoneIntelligence(YouRataConfiguration appConfig, MilestoneIntelligenceRegistry milestoneIntelligence, ActionReportActionIntelligence actionIntelligence)
    {
        long updateTime = DateTimeOffset.Now.ToUnixTimeSeconds();
        milestoneIntelligence.ActionReport.Condition = actionIntelligence.Condition;
        milestoneIntelligence.ActionReport.ProcessId = actionIntelligence.ProcessId;
        if (milestoneIntelligence.ActionReport.StartTime == 0)
        {
            milestoneIntelligence.ActionReport.StartTime = updateTime;
        }
        milestoneIntelligence.ActionReport.LastUpdate = updateTime;
    }

    internal string GetConfigJson(YouRataConfiguration appConfig)
    {
        return JsonConvert.SerializeObject(appConfig, Formatting.None);
    }

    internal void LogMessage(string message)
    {
        StringBuilder lineBuilder = new StringBuilder();
        lineBuilder.Append("[");
        lineBuilder.Append(DateTime.Now.ToString(TimeConstants.ZuluTimeFormat, CultureInfo.InvariantCulture));
        lineBuilder.Append("] ");
        lineBuilder.Append("[Message] ");
        lineBuilder.Append(message);
        AppendLog(lineBuilder.ToString());
    }

    internal void AppendLog(string message)
    {
        lock (_logBuilder)
        {
            _logBuilder.Add(message);
        }
    }

    internal List<string> GetLogs()
    {
        return _logBuilder;
    }

    internal void ClearLogs()
    {
        _logBuilder.Clear();
    }
}
