// Copyright (c) 2023 battleship-systems.
// Licensed under the MIT license.

using System;
using System.IO;
using Newtonsoft.Json;
using YouRata.Common;
using YouRata.Common.ActionReport;

namespace YouRata.ConflictMonitor.ActionReport;

internal class PreviousActionReportProvider
{
    private readonly ActionReportRoot _actionReportRoot;
    private readonly bool _missingActionReport;

    public PreviousActionReportProvider()
    {
        _missingActionReport = true;
        _actionReportRoot = new ActionReportRoot();
        string? workspace = Environment.GetEnvironmentVariable(YouRataConstants.GitHubWorkspaceVariable);
        if (workspace == null) return;
        string fileName = Path.Combine(workspace, YouRataConstants.ActionReportFileName);
        try
        {
            ActionReportRoot? deserializeActionReportRoot = JsonConvert.DeserializeObject<ActionReportRoot>(File.ReadAllText(fileName));
            if (deserializeActionReportRoot != null)
            {
                _actionReportRoot = deserializeActionReportRoot;
                _missingActionReport = false;
            }
        }
        catch
        {
            Console.WriteLine("No previous action report found");
        }
    }

    public ActionReportLayout ActionReport => _actionReportRoot.ActionReport;

    public bool IsMissing => _missingActionReport;
}
