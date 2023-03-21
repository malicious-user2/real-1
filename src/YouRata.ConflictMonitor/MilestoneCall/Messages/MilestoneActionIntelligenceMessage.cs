using System;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using YouRata.Common.Configurations;
using YouRata.Common.GitHub;
using YouRata.Common.Proto;
using YouRata.ConflictMonitor.MilestoneData;
using static YouRata.Common.Proto.MilestoneActionIntelligence.Types;

namespace YouRata.ConflictMonitor.MilestoneCall.Messages;

internal class MilestoneActionIntelligenceMessage : MilestoneActionIntelligenceService.MilestoneActionIntelligenceServiceBase
{
    private readonly ILogger<MilestoneActionIntelligenceMessage> _logger;
    private readonly CallManager _callManager;
    private readonly IOptions<YouRataConfiguration> _configuration;
    private readonly IOptions<GitHubEnvironment> _environment;
    private readonly MilestoneIntelligenceRegistry _milestoneIntelligence;

    public MilestoneActionIntelligenceMessage(ILoggerFactory loggerFactory, CallManager callManager, IOptions<YouRataConfiguration> configuration, IOptions<GitHubEnvironment> environment, MilestoneIntelligenceRegistry milestoneIntelligence)
    {
        _logger = loggerFactory.CreateLogger<MilestoneActionIntelligenceMessage>();
        _callManager = callManager;
        _configuration = configuration;
        _environment = environment;
        _milestoneIntelligence = milestoneIntelligence;
    }

    public override Task<Empty> UpdateInitialSetupActionIntelligence(InitialSetupActionIntelligence request, ServerCallContext context)
    {
        TaskCompletionSource<Empty> emptyResult = new TaskCompletionSource<Empty>();
        _callManager.ActionCallbacks.Enqueue((CallHandler callHandler) =>
        {
            try
            {
                callHandler.UpdateInitialSetupActionIntelligence(_configuration.Value, _milestoneIntelligence, request);
            }
            catch (Exception e)
            {
                _logger.LogError($"Error on UpdateInitialSetupActionIntelligence: {e.Message}");
            }
            emptyResult.SetResult(new Empty());
        });
        _callManager.ActionReady.Set();
        return emptyResult.Task;
    }

    public override Task<Empty> KeepaliveInitialSetup(Empty request, ServerCallContext context)
    {
        TaskCompletionSource<Empty> emptyResult = new TaskCompletionSource<Empty>();
        _callManager.ActionCallbacks.Enqueue((callHandler) =>
        {
            try
            {
                callHandler.KeepaliveInitialSetup(_milestoneIntelligence);
            }
            catch (Exception e)
            {
                _logger.LogError($"Error on KeepaliveInitialSetup: {e.Message}");
            }
            emptyResult.SetResult(new Empty());
        });
        _callManager.ActionReady.Set();
        return emptyResult.Task;
    }

    public override Task<Empty> UpdateYouTubeSyncActionIntelligence(YouTubeSyncActionIntelligence request, ServerCallContext context)
    {
        TaskCompletionSource<Empty> emptyResult = new TaskCompletionSource<Empty>();
        _callManager.ActionCallbacks.Enqueue((CallHandler callHandler) =>
        {
            try
            {
                callHandler.UpdateYouTubeSyncActionIntelligence(_configuration.Value, _milestoneIntelligence, request);
            }
            catch (Exception e)
            {
                _logger.LogError($"Error on UpdateYouTubeSyncActionIntelligence: {e.Message}");
            }
            emptyResult.SetResult(new Empty());
        });
        _callManager.ActionReady.Set();
        return emptyResult.Task;
    }

    public override Task<Empty> KeepaliveYouTubeSync(Empty request, ServerCallContext context)
    {
        TaskCompletionSource<Empty> emptyResult = new TaskCompletionSource<Empty>();
        _callManager.ActionCallbacks.Enqueue((callHandler) =>
        {
            try
            {
                callHandler.KeepaliveYouTubeSync(_milestoneIntelligence);
            }
            catch (Exception e)
            {
                _logger.LogError($"Error on KeepaliveYouTubeSync: {e.Message}");
            }
            emptyResult.SetResult(new Empty());
        });
        _callManager.ActionReady.Set();
        return emptyResult.Task;
    }

    public override Task<Empty> UpdateActionReportActionIntelligence(ActionReportActionIntelligence request, ServerCallContext context)
    {
        TaskCompletionSource<Empty> emptyResult = new TaskCompletionSource<Empty>();
        _callManager.ActionCallbacks.Enqueue((CallHandler callHandler) =>
        {
            try
            {
                callHandler.UpdateActionReportMilestoneIntelligence(_configuration.Value, _milestoneIntelligence, request);
            }
            catch (Exception e)
            {
                _logger.LogError($"Error on UpdateActionReportActionIntelligence: {e.Message}");
            }
            emptyResult.SetResult(new Empty());
        });
        if (request.Condition == MilestoneCondition.MilestoneCompleted)
        {
            _callManager.ActionStop.Set();
        }
        _callManager.ActionReady.Set();
        return emptyResult.Task;
    }

    public override Task<Empty> KeepaliveActionReport(Empty request, ServerCallContext context)
    {
        TaskCompletionSource<Empty> emptyResult = new TaskCompletionSource<Empty>();
        _callManager.ActionCallbacks.Enqueue((callHandler) =>
        {
            try
            {
                callHandler.KeepaliveActionReport(_milestoneIntelligence);
            }
            catch (Exception e)
            {
                _logger.LogError($"Error on KeepaliveActionReport: {e.Message}");
            }
            emptyResult.SetResult(new Empty());
        });
        _callManager.ActionReady.Set();
        return emptyResult.Task;
    }
}
