using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using YouRata.Common.Configurations;
using YouRata.Common.GitHub;
using YouRata.ConflictMonitor.ActionReport;
using YouRata.ConflictMonitor.MilestoneCall;
using YouRata.ConflictMonitor.MilestoneCall.Messages;
using YouRata.ConflictMonitor.MilestoneData;
using YouRata.ConflictMonitor.MilestoneInterface.ConfigurationValidation;
using YouRata.ConflictMonitor.MilestoneInterface.Services;
using YouRata.ConflictMonitor.MilestoneInterface.WebHost;
using YouRata.ConflictMonitor.MilestoneProcess;
using YouRata.ConflictMonitor.Workflow;
using static YouRata.ConflictMonitor.MilestoneCall.CallManager;

namespace YouRata.ConflictMonitor.MilestoneInterface;

internal class WebAppServer
{
    private readonly CallHandler _callHandler;
    private readonly CallManager _callManager;
    private readonly InServiceLoggerProvider _loggerProvider;
    private readonly ConfigurationHelper _configurationHelper;
    private readonly GitHubEnvironmentHelper _environmentHelper;
    private readonly ConflictMonitorWorkflow _conflictMonitorWorkflow;
    private readonly MilestoneIntelligenceRegistry _milestoneIntelligence;
    private readonly PreviousActionReportProvider _actionReportProvider;

    public WebAppServer(CallHandler callHandler, InServiceLoggerProvider loggerProvider, ConfigurationHelper configurationHelper, GitHubEnvironmentHelper environmentHelper, ConflictMonitorWorkflow conflictMonitorWorkflow, MilestoneIntelligenceRegistry milestoneIntelligence, PreviousActionReportProvider actionReportProvider)
    {
        _callHandler = callHandler;
        _callManager = new CallManager();
        _loggerProvider = loggerProvider;
        _configurationHelper = configurationHelper;
        _environmentHelper = environmentHelper;
        _conflictMonitorWorkflow = conflictMonitorWorkflow;
        _milestoneIntelligence = milestoneIntelligence;
        _actionReportProvider = actionReportProvider;
    }

    private WebApplication BuildApp()
    {
        IConfiguration appConfig = _configurationHelper.Build();
        IConfiguration environmentConfig = _environmentHelper.Build();
        WebApplicationBuilder builder = WebApplication.CreateBuilder();
        builder.Services.AddGrpc(opt =>
        {
            opt.EnableDetailedErrors = true;
            opt.ResponseCompressionLevel = GrpcConstants.ResponseCompressionLevel;
        });
        builder.WebHost.AddUnixSocket();
        builder.Logging.Services.AddSingleton<ILoggerProvider, InServiceLoggerProvider>(service => _loggerProvider);
        builder.Services.AddAppConfiguration(appConfig);
        builder.Services.AddGitHubEnvironment(environmentConfig);
        builder.Services.AddSingleton<ConflictMonitorWorkflow>(service => _conflictMonitorWorkflow);
        builder.Services.AddSingleton<MilestoneIntelligenceRegistry>(service => _milestoneIntelligence);
        builder.Services.AddSingleton<PreviousActionReportProvider>(service => _actionReportProvider);
        builder.Services.AddSingleton<CallManager>(service => _callManager);
        WebApplication app = builder.Build();
        return app;
    }

    internal async Task RunAsync()
    {
        WebApplication webApp = BuildApp();
        webApp.MapGrpcService<ActionIntelligenceMessage>();
        webApp.MapGrpcService<MilestoneActionIntelligenceMessage>();
        webApp.MapGrpcService<LogMessage>();

        await webApp.ValidateConfigurationAsync().ConfigureAwait(false);

        await webApp.StartAsync().ConfigureAwait(false);

        using (MilestoneLifetimeManager lifetimeManager = new MilestoneLifetimeManager(webApp, _milestoneIntelligence))
        {
            lifetimeManager.StartLoop();
            while (true)
            {
                _callManager.ActionReady.WaitOne();
                while (!_callManager.ActionCallbacks.IsEmpty)
                {
                    _callManager.ActionCallbacks.TryDequeue(out ConflictMonitorCall? call);
                    if (call != null)
                    {
                        call(_callHandler);
                    }
                }
                if (_callManager.ActionStop.WaitOne(0))
                {
                    await webApp.StopAsync().ConfigureAwait(false);
                    break;
                }
            }
        }
    }
}
