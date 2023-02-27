using System.Collections.Concurrent;
using System.Threading;

namespace YouRata.ConflictMonitor.MilestoneCall;

internal class CallManager
{
    internal delegate void ConflictMonitorCall(CallHandler handler);
    private readonly AutoResetEvent _actionReady;
    private readonly ManualResetEvent _actionStop;
    private readonly ConcurrentQueue<ConflictMonitorCall> _actionCallbacks;

    public CallManager()
    {
        _actionReady = new AutoResetEvent(false);
        _actionStop = new ManualResetEvent(false);
        _actionCallbacks = new ConcurrentQueue<ConflictMonitorCall>();
    }

    public AutoResetEvent ActionReady => _actionReady;
    public ManualResetEvent ActionStop => _actionStop;
    public ConcurrentQueue<ConflictMonitorCall> ActionCallbacks => _actionCallbacks;
}
