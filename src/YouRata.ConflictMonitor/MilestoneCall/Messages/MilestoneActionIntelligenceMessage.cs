using System;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using YouRata.Common.Configuration;
using YouRata.Common.GitHub;
using YouRata.Common.Proto;
using YouRata.ConflictMonitor.MilestoneData;
using static YouRata.Common.Proto.MilestoneActionIntelligence.Types;

namespace YouRata.ConflictMonitor.MilestoneCall.Messages;

internal class MilestoneActionIntelligenceMessage : MilestoneActionIntelligenceService.MilestoneActionIntelligenceServiceBase
{
    private readonly ILogger<MilestoneActionIntelligenceMessage> _logger;
    private readonly CallManager _callManager;
    private readonly MilestoneIntelligenceRegistry _milestoneIntelligence;

    public MilestoneActionIntelligenceMessage(ILoggerFactory loggerFactory, CallManager callManager, MilestoneIntelligenceRegistry milestoneIntelligence)
    {
        _logger = loggerFactory.CreateLogger<MilestoneActionIntelligenceMessage>();
        _callManager = callManager;
        _milestoneIntelligence = milestoneIntelligence;
    }

    public override Task<Empty> UpdateInitialSetupActionIntelligence(InitialSetupActionIntelligence request, ServerCallContext context)
    {
        TaskCompletionSource<Empty> emptyResult = new TaskCompletionSource<Empty>();
        _callManager.ActionCallbacks.Enqueue((CallHandler callHandler) =>
        {
            try
            {
                callHandler.UpdateInitialSetupActionIntelligence(_milestoneIntelligence, request);
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
        _logger.LogError("This is a fake error");
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
                callHandler.UpdateYouTubeSyncActionIntelligence(_milestoneIntelligence, request);
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
                callHandler.UpdateActionReportMilestoneIntelligence(_milestoneIntelligence, request);
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
