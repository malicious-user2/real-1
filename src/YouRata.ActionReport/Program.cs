using System;
using YouRata.ActionReport.ActionReportFile;
using YouRata.ActionReport.ConflictMonitor;
using YouRata.Common;
using YouRata.Common.Configurations;
using YouRata.Common.GitHub;
using YouRata.Common.Milestone;
using YouRata.Common.Proto;
using static YouRata.Common.Proto.MilestoneActionIntelligence.Types;

using (ActionReportCommunicationClient client = new ActionReportCommunicationClient())
{
    if (!client.Activate(out ActionReportActionIntelligence milestoneInt)) return;
    if (client.GetYouRataConfiguration().ActionCutOuts.DisableActionReportMilestone) return;
    MilestoneVariablesHelper.CreateRuntimeVariables(client, out ActionIntelligence actionInt, out YouRataConfiguration config, out GitHubActionEnvironment actionEnvironment);



    try
    {
        ActionReportBuilder builder = new ActionReportBuilder(client.GetActionIntelligence());

        GitHubAPIClient.UpdateContentFile(actionEnvironment.OverrideRateLimit(), YouRataConstants.ActionReportMessage, builder.Build(), YouRataConstants.ActionReportFileName, client.LogMessage);
    }
    catch (Exception ex)
    {
        client.SetStatus(MilestoneCondition.MilestoneFailed);
        throw new MilestoneException("ActionReport failed", ex);
    }
    client.SetStatus(MilestoneCondition.MilestoneCompleted);
}
