using System;
using System.IO;
using Newtonsoft.Json;
using YouRata.Common;
using YouRata.Common.ActionReport;
using YouRata.Common.GitHub;

namespace YouRata.ConflictMonitor.ActionReport;

internal class PreviousActionReportProvider
{
    private readonly ActionReportRoot _actionReportRoot;
    private readonly bool _missingActionReport;

    public PreviousActionReportProvider()
    {
        _actionReportRoot = new ActionReportRoot();
        string? workspace = Environment.GetEnvironmentVariable(GitHubConstants.GitHubWorkspaceVariable);
        if (workspace != null)
        {
            string fileName = Path.Combine(workspace, GitHubConstants.ErrataCheckoutPath, YouRataConstants.ActionReportFileName);
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
                _missingActionReport = true;
            }
        }
    }

    public ActionReportLayout ActionReport => _actionReportRoot.ActionReport;

    public bool IsMissing => _missingActionReport;
}
