using System.Globalization;
using System.Text;

namespace YouRatta.ConflictMonitor.MilestoneCall;

public class CallHandler
{
    private readonly StringBuilder _logBuilder = new StringBuilder();

    public CallHandler()
    {
        _logBuilder = new StringBuilder();
    }

    public void AppendLog(string message)
    {
        lock (_logBuilder)
        {
            _logBuilder.AppendLine(string.Format(CultureInfo.InvariantCulture, "{0}", message));
        }
    }

    public string GetLogs()
    {
        return _logBuilder.ToString();
    }

    public void ClearLogs()
    {
        _logBuilder.Clear();
    }
}
