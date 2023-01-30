using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using YouRatta.ConflictMonitor.MilestoneCall;
using YouRatta.ConflictMonitor.MilestoneCall.Messages;
using static YouRatta.ConflictMonitor.MilestoneCall.CallManager;

namespace YouRatta.ConflictMonitor.MilestoneInterface;

internal class WebAppServer
{
    private readonly CallHandler _callHandler;
    private readonly CallManager _callManager;
    private readonly InServiceLoggerProvider _loggerProvider;

    public WebAppServer(CallHandler callHandler, InServiceLoggerProvider logger)
    {
        _callHandler = callHandler;
        _callManager = new CallManager();
        _loggerProvider = logger;
    }

    private WebApplication BuildApp()
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder();
        builder.Services.AddGrpc(opt =>
        {
            opt.EnableDetailedErrors = true;
            opt.ResponseCompressionLevel = GrpcConstants.ResponseCompressionLevel;
        });
        builder.WebHost.ConfigureKestrel(opt =>
        {
            if (File.Exists(GrpcConstants.UnixSocketPath))
            {
                File.Delete(GrpcConstants.UnixSocketPath);
            }
            opt.ListenUnixSocket(GrpcConstants.UnixSocketPath);
        });
        builder.Logging.Services.AddSingleton<ILoggerProvider, InServiceLoggerProvider>(service => _loggerProvider);
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
