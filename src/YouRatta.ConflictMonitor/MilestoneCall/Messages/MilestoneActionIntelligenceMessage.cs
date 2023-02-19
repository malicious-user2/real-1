using System;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using YouRatta.Common.Configurations;
using YouRatta.Common.GitHub;
using YouRatta.Common.Proto;
using YouRatta.ConflictMonitor.MilestoneData;
using static YouRatta.Common.Proto.MilestoneActionIntelligence.Types;

namespace YouRatta.ConflictMonitor.MilestoneCall.Messages;

internal class MilestoneActionIntelligenceMessage : MilestoneActionIntelligenceService.MilestoneActionIntelligenceServiceBase
{
    private readonly ILogger<MilestoneActionIntelligenceMessage> _logger;
    private readonly CallManager _callManager;
    private readonly IOptions<YouRattaConfiguration> _configuration;
    private readonly IOptions<GitHubEnvironment> _environment;
    private readonly MilestoneIntelligenceRegistry _milestoneIntelligence;

    public MilestoneActionIntelligenceMessage(ILoggerFactory loggerFactory, CallManager callManager, IOptions<YouRattaConfiguration> configuration, IOptions<GitHubEnvironment> environment, MilestoneIntelligenceRegistry milestoneIntelligence)
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
                callHandler.UpdateInitialSetupMilestoneIntelligence(_configuration.Value, _milestoneIntelligence, request);
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
}
