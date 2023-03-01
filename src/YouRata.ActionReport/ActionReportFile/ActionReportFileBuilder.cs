using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using YouRata.Common.Proto;

namespace YouRata.ActionReport.ActionReportFile;

internal class ActionReportFileBuilder
{
    private readonly ActionIntelligence _actionIntelligence;

    internal ActionReportFileBuilder(ActionIntelligence actionIntelligence)
    {
        _actionIntelligence = actionIntelligence;
    }

    public string Build()
    {
        JObject yr = new JObject
        {
            ["Status"] = "finished",
            ["LastUpdate"] = "now",
            ["Logs"] = _actionIntelligence.LogMessages.ToString()
        };
        return yr.ToString(Formatting.Indented);
    }
}
