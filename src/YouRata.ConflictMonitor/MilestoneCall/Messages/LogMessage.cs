using System;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using YouRata.Common.Proto;

namespace YouRata.ConflictMonitor.MilestoneCall.Messages;

internal class LogMessage : LogService.LogServiceBase
{
    private readonly ILogger<LogMessage> _logger;
    private readonly CallManager _callManager;

    public LogMessage(ILoggerFactory loggerFactory, CallManager callManager)
    {
        _logger = loggerFactory.CreateLogger<LogMessage>();
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

    public override Task<LogMessages> GetLogMessages(Empty request, ServerCallContext context)
    {
        TaskCompletionSource<LogMessages> logMessagesResult = new TaskCompletionSource<LogMessages>();
        _callManager.ActionCallbacks.Enqueue((callHandler) =>
        {
            LogMessages logMessages = new LogMessages();
            try
            {
                logMessages.Messages.AddRange(callHandler.GetLogs());
            }
            catch (Exception e)
            {
                _logger.LogError($"Error on GetLogMessages: {e.Message}");
            }
            logMessagesResult.SetResult(logMessages);
        });
        _callManager.ActionReady.Set();
        return logMessagesResult.Task;
    }
}
