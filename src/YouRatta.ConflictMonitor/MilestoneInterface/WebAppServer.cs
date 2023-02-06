using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using YouRatta.Common.Configurations;
using YouRatta.Common.GitHub;
using YouRatta.ConflictMonitor.MilestoneData;
using YouRatta.ConflictMonitor.MilestoneCall;
using YouRatta.ConflictMonitor.MilestoneCall.Messages;
using YouRatta.ConflictMonitor.MilestoneInterface.Services;
using YouRatta.ConflictMonitor.MilestoneInterface.WebHost;
using YouRatta.ConflictMonitor.Workflow;
using static YouRatta.ConflictMonitor.MilestoneCall.CallManager;
using YouRatta.ConflictMonitor.MilestoneProcess;
using YouRatta.ConflictMonitor.MilestoneInterface.ConfigurationValidation;

namespace YouRatta.ConflictMonitor.MilestoneInterface;

internal class WebAppServer
{
    private readonly CallHandler _callHandler;
    private readonly CallManager _callManager;
    private readonly InServiceLoggerProvider _loggerProvider;
    private readonly ConfigurationHelper _configurationHelper;
    private readonly GitHubEnvironmentHelper _environmentHelper;
    private readonly ConflictMonitorWorkflow _conflictMonitorWorkflow;
    private readonly MilestoneIntelligenceRegistry _milestoneIntelligence;

    public WebAppServer(CallHandler callHandler, InServiceLoggerProvider loggerProvider, ConfigurationHelper configurationHelper, GitHubEnvironmentHelper environmentHelper, ConflictMonitorWorkflow conflictMonitorWorkflow, MilestoneIntelligenceRegistry milestoneIntelligence)
    {
        _callHandler = callHandler;
        _callManager = new CallManager();
        _loggerProvider = loggerProvider;
        _configurationHelper = configurationHelper;
        _environmentHelper = environmentHelper;
        _conflictMonitorWorkflow = conflictMonitorWorkflow;
        _milestoneIntelligence = milestoneIntelligence;
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
        builder.Services.AddConfigurationWriter(_configurationHelper.SettingsFilePath);



        builder.Services.AddSingleton<ConflictMonitorWorkflow>(service => _conflictMonitorWorkflow);
        builder.Services.AddSingleton<MilestoneIntelligenceRegistry>(service => _milestoneIntelligence);
        builder.Services.AddSingleton<CallManager>(service => _callManager);
        WebApplication app = builder.Build();
        return app;
    }

    internal async Task RunAsync()
    {
        WebApplication webApp = BuildApp();
        webApp.MapGrpcService<ActionIntelligenceMessage>();
        webApp.MapGrpcService<MilestoneActionIntelligenceMessage>();

        await webApp.ValidateConfigurationAsync().ConfigureAwait(false);

        await webApp.StartAsync().ConfigureAwait(false);

        MilestoneLifetimeConfiguration? lifetimeConfig = webApp.Configuration.Get<MilestoneLifetimeConfiguration>();
        if (lifetimeConfig == null) return;

        using (MilestoneLifetimeManager lifetimeManager = new MilestoneLifetimeManager(lifetimeConfig, _milestoneIntelligence))
        {
            lifetimeManager.Start();
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
            }
        }

    }
}
