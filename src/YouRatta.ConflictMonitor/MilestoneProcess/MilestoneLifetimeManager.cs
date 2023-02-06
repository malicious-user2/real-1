using System;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using YouRatta.Common.Configurations;
using YouRatta.ConflictMonitor.MilestoneData;
using static YouRatta.Common.Proto.MilestoneActionIntelligence.Types;

namespace YouRatta.ConflictMonitor.MilestoneProcess;

internal class MilestoneLifetimeManager : IDisposable
{
    private readonly MilestoneLifetimeConfiguration _configuration;
    private readonly MilestoneIntelligenceRegistry _milestoneIntelligence;
    private readonly object _lock = new object();
    private bool _disposed;
    private readonly CancellationTokenSource _cancelTokenSource;

    internal MilestoneLifetimeManager(MilestoneLifetimeConfiguration configuration, MilestoneIntelligenceRegistry milestoneIntelligence)
    {
        _configuration = configuration;
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
                if (_milestoneIntelligence.InitialSetup.Condition == MilestoneCondition.MilestoneRunning &&
                    _milestoneIntelligence.InitialSetup.LastUpdate != 0 &&
                    _milestoneIntelligence.InitialSetup.StartTime != 0 &&
                    _milestoneIntelligence.InitialSetup.ProcessId !=0)
                {
                    long dwellTime = DateTimeOffset.Now.ToUnixTimeSeconds() - _milestoneIntelligence.InitialSetup.LastUpdate;
                    long runTime = DateTimeOffset.Now.ToUnixTimeSeconds() - _milestoneIntelligence.InitialSetup.StartTime;
                    if (dwellTime > _configuration.MaxUpdateDwellTime ||
                        runTime > _configuration.MaxRunTime)
                    {
                        Process milestoneProcess = Process.GetProcessById(_milestoneIntelligence.InitialSetup.ProcessId);
                        if (milestoneProcess != null)
                        {
                            milestoneProcess.Kill();
                            _milestoneIntelligence.InitialSetup.Condition = MilestoneCondition.MilestoneFailed;
                        }
                    }
                }
            }
        }
    }
}
