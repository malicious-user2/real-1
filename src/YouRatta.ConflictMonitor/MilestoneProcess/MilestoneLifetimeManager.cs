using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using YouRatta.Common.Configurations;
using YouRatta.ConflictMonitor.MilestoneData;
using static YouRatta.Common.Proto.MilestoneActionIntelligence.Types;

namespace YouRatta.ConflictMonitor.MilestoneProcess;

internal class MilestoneLifetimeManager : IDisposable
{
    private readonly WebApplication _webApp;
    private readonly MilestoneIntelligenceRegistry _milestoneIntelligence;
    private readonly object _lock = new object();
    private bool _disposed;
    private readonly CancellationTokenSource _cancelTokenSource;

    internal MilestoneLifetimeManager(WebApplication webApp, MilestoneIntelligenceRegistry milestoneIntelligence)
    {
        _webApp = webApp;
        _milestoneIntelligence = milestoneIntelligence;
        _cancelTokenSource = new CancellationTokenSource();
    }

    public void Dispose()
    {
        lock (_lock)
        {
            if (!_disposed)
            {
                _cancelTokenSource.Cancel();
                _cancelTokenSource.Dispose();
                _disposed = true;
            }
        }
    }

    internal void Start()
    {
        lock (_lock)
        {
            if (!_disposed)
            {
                Task.Run(() =>
                {
                    while (!_disposed && !_cancelTokenSource.Token.IsCancellationRequested)
                    {
                        Task.Delay(MilestoneLifetimeConstants.LifetimeCheckInterval, _cancelTokenSource.Token);
                        ProcessLifetimeManager();
                    }
                });
            }
        }
    }

    private void ProcessLifetimeManager()
    {
        lock (_lock)
        {
            if (!_disposed)
            {
                IOptions<YouRattaConfiguration>? options = _webApp.Services.GetService<IOptions<YouRattaConfiguration>>();
                MilestoneLifetimeConfiguration config = options.Value.MilestoneLifetime;
                foreach (BaseMilestoneIntelligence milestoneIntelligence in _milestoneIntelligence.Milestones)
                {
                    if (milestoneIntelligence.Condition == MilestoneCondition.MilestoneRunning &&
                    milestoneIntelligence.LastUpdate != 0 &&
                    milestoneIntelligence.StartTime != 0 &&
                    milestoneIntelligence.ProcessId != 0)
                    {

                        long dwellTime = DateTimeOffset.Now.ToUnixTimeSeconds() - milestoneIntelligence.LastUpdate;
                        long runTime = DateTimeOffset.Now.ToUnixTimeSeconds() - milestoneIntelligence.StartTime;
                        if (dwellTime > config.MaxUpdateDwellTime ||
                            runTime > config.MaxRunTime)
                        {
                            Process milestoneProcess = Process.GetProcessById(milestoneIntelligence.ProcessId);
                            if (milestoneProcess != null)
                            {
                                milestoneProcess.Kill();
                                milestoneIntelligence.Condition = MilestoneCondition.MilestoneFailed;
                            }
                        }
                    }
                }
            }
        }
    }
}
