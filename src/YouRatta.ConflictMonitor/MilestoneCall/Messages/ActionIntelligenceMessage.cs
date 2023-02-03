using System;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using YouRatta.Common.Configurations;
using YouRatta.Common.Proto;

namespace YouRatta.ConflictMonitor.MilestoneCall.Messages;

internal class ActionIntelligenceMessage : ActionIntelligenceService.ActionIntelligenceServiceBase
{
    private readonly ILogger<ActionIntelligenceMessage> _logger;
    private readonly CallManager _callManager;
    private readonly IOptions<YouRattaConfiguration> _configuration;

    public ActionIntelligenceMessage(ILoggerFactory loggerFactory, CallManager callManager, IOptions<YouRattaConfiguration> configuration)
    {
        _logger = loggerFactory.CreateLogger<ActionIntelligenceMessage>();
        _callManager = callManager;
        _configuration = configuration;
    }

    public override Task<ActionIntelligence> GetActionIntelligence(Empty request, ServerCallContext context)
    {
        TaskCompletionSource<ActionIntelligence> actionIntelligenceResult = new TaskCompletionSource<ActionIntelligence>();
        _callManager.ActionCallbacks.Enqueue((CallHandler callHandler) =>
        {
            ActionIntelligence actionIntelligence = new ActionIntelligence();
            try
            {
                if (_configuration.Value.ActionEnabled)
                {
                    callHandler.GetLogs();
                }

                //do something
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
