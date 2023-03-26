// Copyright (c) 2023 battleship-systems.
// Licensed under the MIT license.

using System;
using System.Collections.Concurrent;
using System.Threading;

namespace YouRata.ConflictMonitor.MilestoneCall;

internal class CallManager
{
    private readonly ConcurrentQueue<ConflictMonitorCall> _actionCallbacks;

    private readonly AutoResetEvent _actionReady;

    private readonly ManualResetEvent _actionStop;

    public CallManager()
    {
        _actionReady = new AutoResetEvent(false);
        _actionStop = new ManualResetEvent(false);
        _actionCallbacks = new ConcurrentQueue<ConflictMonitorCall>();
    }

    public ConcurrentQueue<ConflictMonitorCall> ActionCallbacks => _actionCallbacks;
    public AutoResetEvent ActionReady => _actionReady;
    public ManualResetEvent ActionStop => _actionStop;

    internal delegate void ConflictMonitorCall(CallHandler handler);
}
