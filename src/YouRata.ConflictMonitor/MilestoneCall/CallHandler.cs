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
        if (!appConfig.ActionCutOuts.DisableConflictMonitorGitHubOperations && workflow.GitHubToken?.Length > 0 && workflow.ApiToken?.Length > 0)
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

    internal string GetPreviousActionReport(YouRataConfiguration appConfig, PreviousActionReportProvider actionReportProvider)
    {
        if (actionReportProvider.ActionReport != null && !actionReportProvider.IsMissing)
        {
            return JsonConvert.SerializeObject(actionReportProvider.ActionReport, Formatting.None);
        }
        return string.Empty;
    }

    internal MilestoneActionIntelligence GetMilestoneActionIntelligence(YouRataConfiguration appConfig, MilestoneIntelligenceRegistry milestoneIntelligence)
    {
        MilestoneActionIntelligence actionIntelligence = new MilestoneActionIntelligence
        {
            InitialSetup = new InitialSetupActionIntelligence
            {
                Condition = milestoneIntelligence.InitialSetup.Condition,
                ProcessId = milestoneIntelligence.InitialSetup.ProcessId
            },
            YouTubeSync = new YouTubeSyncActionIntelligence
            {
                Condition = milestoneIntelligence.YouTubeSync.Condition,
                ProcessId = milestoneIntelligence.YouTubeSync.ProcessId,
                VideosProcessed = milestoneIntelligence.YouTubeSync.VideosProcessed,
                VideosSkipped = milestoneIntelligence.YouTubeSync.VideosSkipped,
                CalculatedQueriesPerDayRemaining =
                    milestoneIntelligence.YouTubeSync.CalculatedQueriesPerDayRemaining,
                LastQueryTime = milestoneIntelligence.YouTubeSync.LastQueryTime,
                FirstVideoPublishTime = milestoneIntelligence.YouTubeSync.FirstVideoPublishTime,
                OutstandingVideoPublishTime = milestoneIntelligence.YouTubeSync.OutstandingVideoPublishTime,
                HasOutstandingVideos = milestoneIntelligence.YouTubeSync.HasOutstandingVideos
            },
            ActionReport = new ActionReportActionIntelligence
            {
                Condition = milestoneIntelligence.ActionReport.Condition,
                ProcessId = milestoneIntelligence.ActionReport.ProcessId
            }
        };

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

    internal void KeepaliveInitialSetup(MilestoneIntelligenceRegistry milestoneIntelligence)
    {
        long updateTime = DateTimeOffset.Now.ToUnixTimeSeconds();
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
        milestoneIntelligence.YouTubeSync.FirstVideoPublishTime = actionIntelligence.FirstVideoPublishTime;
        milestoneIntelligence.YouTubeSync.OutstandingVideoPublishTime = actionIntelligence.OutstandingVideoPublishTime;
        milestoneIntelligence.YouTubeSync.HasOutstandingVideos = actionIntelligence.HasOutstandingVideos;
    }

    internal void KeepaliveYouTubeSync(MilestoneIntelligenceRegistry milestoneIntelligence)
    {
        long updateTime = DateTimeOffset.Now.ToUnixTimeSeconds();
        milestoneIntelligence.YouTubeSync.LastUpdate = updateTime;
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

    internal void KeepaliveActionReport(MilestoneIntelligenceRegistry milestoneIntelligence)
    {
        long updateTime = DateTimeOffset.Now.ToUnixTimeSeconds();
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
        lineBuilder.Append(DateTime.Now.ToString(YouRataConstants.ZuluTimeFormat, CultureInfo.InvariantCulture));
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
}
