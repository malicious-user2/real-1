using System;
using System.Threading.Tasks;
using Google.Protobuf.Collections;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using YouRatta.Common.Configurations;
using YouRatta.Common.GitHub;
using YouRatta.Common.Proto;
using YouRatta.ConflictMonitor.MilestoneData;
using YouRatta.ConflictMonitor.Workflow;

namespace YouRatta.ConflictMonitor.MilestoneCall.Messages;

internal class ActionIntelligenceMessage : ActionIntelligenceService.ActionIntelligenceServiceBase
{
    private readonly ILogger<ActionIntelligenceMessage> _logger;
    private readonly CallManager _callManager;
    private readonly IOptions<YouRattaConfiguration> _configuration;
    private readonly IOptions<GitHubEnvironment> _environment;
    private readonly ConflictMonitorWorkflow _conflictMonitorWorkflow;
    private readonly MilestoneIntelligenceRegistry _milestoneIntelligence;

    public ActionIntelligenceMessage(ILoggerFactory loggerFactory, CallManager callManager, IOptions<YouRattaConfiguration> configuration, IOptions<GitHubEnvironment> environment, ConflictMonitorWorkflow conflictMonitorWorkflow, MilestoneIntelligenceRegistry milestoneIntelligence)
    {
        _logger = loggerFactory.CreateLogger<ActionIntelligenceMessage>();
        _callManager = callManager;
        _configuration = configuration;
        _environment = environment;
        _conflictMonitorWorkflow = conflictMonitorWorkflow;
        _milestoneIntelligence = milestoneIntelligence;
    }

    public override Task<ActionIntelligence> GetActionIntelligence(Empty request, ServerCallContext context)
    {
        TaskCompletionSource<ActionIntelligence> actionIntelligenceResult = new TaskCompletionSource<ActionIntelligence>();
        _callManager.ActionCallbacks.Enqueue((CallHandler callHandler) =>
        {
            ActionIntelligence actionIntelligence = new ActionIntelligence();
            try
            {
                actionIntelligence.GitHubActionEnvironment = callHandler.GetGithubActionEnvironment(_configuration.Value, _environment.Value, _conflictMonitorWorkflow);
                actionIntelligence.TokenResponse = callHandler.GetStoredTokenResponse(_configuration.Value, _conflictMonitorWorkflow);
                actionIntelligence.MilestoneIntelligence = callHandler.GetMilestoneActionIntelligence(_configuration.Value, _milestoneIntelligence);
                actionIntelligence.ConfigJson = callHandler.GetConfigJson(_configuration.Value);
                actionIntelligence.AppClientId = callHandler.GetAppClientId(_configuration.Value, _conflictMonitorWorkflow);
                actionIntelligence.AppClientSecret = callHandler.GetAppClientSecret(_configuration.Value, _conflictMonitorWorkflow);
                actionIntelligence.LogMessages.AddRange(callHandler.GetLogs());
            }
            catch (Exception e)
            {
                _logger.LogError($"Error on GetActionIntelligence: {e.Message}");
            }
            actionIntelligenceResult.SetResult(actionIntelligence);
        });
        _callManager.ActionReady.Set();
        return actionIntelligenceResult.Task;
    }
}
