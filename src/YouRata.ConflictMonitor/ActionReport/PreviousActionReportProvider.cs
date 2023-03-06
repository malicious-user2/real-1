using System.IO;
using Newtonsoft.Json;
using YouRata.Common;
using YouRata.Common.ActionReport;
using YouRata.Common.GitHub;

namespace YouRata.ConflictMonitor.ActionReport;

internal class PreviousActionReportProvider
{
    private readonly ActionReportRoot _actionReportRoot;

    public PreviousActionReportProvider()
    {
        string fileName = Path.Combine(Directory.GetCurrentDirectory(), GitHubConstants.ErrataCheckoutPath, YouRataConstants.ActionReportFileName);
        _actionReportRoot = new ActionReportRoot();
        try
        {
            ActionReportRoot? deserializeActionReportRoot = JsonConvert.DeserializeObject<ActionReportRoot>(File.ReadAllText(fileName));
            if (deserializeActionReportRoot != null)
            {
                _actionReportRoot = deserializeActionReportRoot;
            }
        }
        catch
        {
        }
    }

    public ActionReportLayout ActionReport => _actionReportRoot.ActionReport;
}
