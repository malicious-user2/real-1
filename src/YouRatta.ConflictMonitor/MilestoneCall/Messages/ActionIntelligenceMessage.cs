using System;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Octokit;
using YouRatta.Common.Configurations;
using YouRatta.Common.GitHub;
using YouRatta.Common.Proto;
using YouRatta.ConflictMonitor.Workflow;

namespace YouRatta.ConflictMonitor.MilestoneCall.Messages;

internal class ActionIntelligenceMessage : ActionIntelligenceService.ActionIntelligenceServiceBase
{
    private readonly ILogger<ActionIntelligenceMessage> _logger;
    private readonly CallManager _callManager;
    private readonly IOptions<YouRattaConfiguration> _configuration;
    private readonly IOptions<GitHubEnvironment> _environment;
    private readonly ConflictMonitorWorkflow _conflictMonitorWorkflow;

    public ActionIntelligenceMessage(ILoggerFactory loggerFactory, CallManager callManager, IOptions<YouRattaConfiguration> configuration, IOptions<GitHubEnvironment> environment, ConflictMonitorWorkflow conflictMonitorWorkflow)
    {
        _logger = loggerFactory.CreateLogger<ActionIntelligenceMessage>();
        _callManager = callManager;
        _configuration = configuration;
        _environment = environment;
        _conflictMonitorWorkflow = conflictMonitorWorkflow;
    }

    public override Task<ActionIntelligence> GetActionIntelligence(Empty request, ServerCallContext context)
    {
        TaskCompletionSource<ActionIntelligence> actionIntelligenceResult = new TaskCompletionSource<ActionIntelligence>();
        _callManager.ActionCallbacks.Enqueue((CallHandler callHandler) =>
        {
            ActionIntelligence actionIntelligence = new ActionIntelligence();
            try
            {
                actionIntelligence.GithubActionEnvironment = _environment.Value.GetActionEnvironment();
                GitHubClient ghClient = new GitHubClient(new Octokit.ProductHeaderValue("TestApp"));
                ghClient.Credentials = new Credentials(_conflictMonitorWorkflow.GithubToken, AuthenticationType.Bearer);


                actionIntelligence.GithubActionEnvironment.RateLimitCoreRemaining = ghClient.RateLimit.GetRateLimits().Result.Resources.Core.Remaining;




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
