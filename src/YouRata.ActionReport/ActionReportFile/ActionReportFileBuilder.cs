using System;
using System.Globalization;
using Google.Protobuf;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using YouRata.Common;
using YouRata.Common.Proto;
using static YouRata.Common.Proto.MilestoneActionIntelligence.Types;

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
        string zuluTime = DateTime.Now.ToString(TimeConstants.ZuluTimeFormat, CultureInfo.InvariantCulture);
        string status = "Unknown";
        JsonFormatter intelligenceFormatter = new JsonFormatter(JsonFormatter.Settings.Default.WithFormatDefaultValues(true));
        MilestoneActionIntelligence milestoneInt = _actionIntelligence.MilestoneIntelligence;


        if (milestoneInt.InitialSetup.Condition != MilestoneCondition.MilestoneFailed &&
            milestoneInt.InitialSetup.Condition != MilestoneCondition.MilestoneBlocked &&
            milestoneInt.YouTubeSync.Condition == MilestoneCondition.MilestoneCompleted)
        {
            status = $"Last Run {zuluTime}";
        }
        else if (milestoneInt.InitialSetup.Condition == MilestoneCondition.MilestoneFailed)
        {
            status = "Initial Setup Failed";
        }
        else if (milestoneInt.YouTubeSync.Condition == MilestoneCondition.MilestoneFailed)
        {
            status = "YouTube Sync Failed";
        }

        JObject actionReport = new JObject
        {
            ["Status"] = status,
            ["InitialSetupIntelligence"] = intelligenceFormatter.Format(milestoneInt.InitialSetup),
            ["YouTubeSyncIntelligence"] = intelligenceFormatter.Format(milestoneInt.YouTubeSync),
            ["ActionReportIntelligence"] = intelligenceFormatter.Format(milestoneInt.ActionReport),
            ["Logs"] = _actionIntelligence.LogMessages.ToString()
        };
        JObject jsonRoot = new JObject
        {
            ["ActionReport"] = actionReport
        };
        return actionReport.ToString(Formatting.Indented);
    }
}
