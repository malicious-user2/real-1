using System;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using YouRata.Common.Proto;

namespace YouRata.ConflictMonitor.MilestoneCall.Messages;

internal class MilestoneLogMessage : MilestoneLogService.MilestoneLogServiceBase
{
    private readonly ILogger<MilestoneLogMessage> _logger;
    private readonly CallManager _callManager;

    public MilestoneLogMessage(ILoggerFactory loggerFactory, CallManager callManager)
    {
        _logger = loggerFactory.CreateLogger<MilestoneLogMessage>();
        _callManager = callManager;
    }

    public override Task<Empty> WriteLogMessage(MilestoneLog request, ServerCallContext context)
    {
        TaskCompletionSource<Empty> emptyResult = new TaskCompletionSource<Empty>();
        _callManager.ActionCallbacks.Enqueue((callHandler) =>
        {
            try
            {
                callHandler.LogMessage($"({request.Milestone}) {request.Message}");
            }
            catch (Exception e)
            {
                _logger.LogError($"Error on WriteLogMessage: {e.Message}");
            }
            emptyResult.SetResult(new Empty());
        });
        _callManager.ActionReady.Set();
        return emptyResult.Task;
    }
}
