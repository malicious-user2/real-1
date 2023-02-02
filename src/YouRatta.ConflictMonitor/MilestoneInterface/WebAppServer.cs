using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using YouRatta.Common.Configurations;
using YouRatta.ConflictMonitor.MilestoneCall;
using YouRatta.ConflictMonitor.MilestoneCall.Messages;
using YouRatta.ConflictMonitor.MilestoneInterface.Services;
using YouRatta.ConflictMonitor.MilestoneInterface.WebHost;
using static YouRatta.ConflictMonitor.MilestoneCall.CallManager;

namespace YouRatta.ConflictMonitor.MilestoneInterface;

internal class WebAppServer
{
    private readonly CallHandler _callHandler;
    private readonly CallManager _callManager;
    private readonly InServiceLoggerProvider _loggerProvider;
    private readonly ConfigurationHelper _configurationHelper;

    public WebAppServer(CallHandler callHandler, InServiceLoggerProvider loggerProvider, ConfigurationHelper configurationHelper)
    {
        _callHandler = callHandler;
        _callManager = new CallManager();
        _loggerProvider = loggerProvider;
        _configurationHelper = configurationHelper;
    }

    private WebApplication BuildApp()
    {
        IConfiguration config = _configurationHelper.Build();
        WebApplicationBuilder builder = WebApplication.CreateBuilder();
        builder.Services.AddGrpc(opt =>
        {
            opt.EnableDetailedErrors = true;
            opt.ResponseCompressionLevel = GrpcConstants.ResponseCompressionLevel;
        });
        builder.WebHost.AddUnixSocket();
        builder.Logging.Services.AddSingleton<ILoggerProvider, InServiceLoggerProvider>(service => _loggerProvider);
        builder.Services.AddAppConfiguration(config);
        builder.Services.AddConfigurationWriter();




        builder.Services.AddSingleton<CallManager>(service => _callManager);
        WebApplication app = builder.Build();
        return app;
    }

    internal async Task RunAsync()
    {
        WebApplication webApp = BuildApp();
        webApp.MapGrpcService<ActionIntelligenceMessage>();

        await webApp.StartAsync().ConfigureAwait(false);

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
