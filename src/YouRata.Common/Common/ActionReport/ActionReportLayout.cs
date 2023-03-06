using System;
using Newtonsoft.Json;

namespace YouRata.Common.ActionReport;

[JsonObject(ItemRequired = Required.Always, Title = "ActionReport")]
public class ActionReportLayout
{
    public ActionReportLayout()
    {
        Status = string.Empty;
        InitialSetupIntelligence = string.Empty;
        YouTubeSyncIntelligence = string.Empty;
        ActionReportIntelligence = string.Empty;
        Logs = string.Empty;
    }

    public string Status { get; set; }

    public string InitialSetupIntelligence { get; set; }

    public string YouTubeSyncIntelligence { get; set; }

    public string ActionReportIntelligence { get; set; }

    public string Logs { get; set; }
}
