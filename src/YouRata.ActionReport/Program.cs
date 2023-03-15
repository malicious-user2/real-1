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
    ActionReportActionIntelligence? milestoneInt = client.GetMilestoneActionIntelligence();
    if (client.GetYouRataConfiguration().ActionCutOuts.DisableActionReportMilestone) return;
    if (milestoneInt == null) return;
    if (milestoneInt.Condition == MilestoneCondition.MilestoneBlocked) return;
    ActionIntelligence actionInt;
    GitHubActionEnvironment actionEnvironment;
    YouRataConfiguration config;
    MilestoneVariablesHelper.CreateRuntimeVariables(client, out actionInt, out config, out actionEnvironment);
    try
    {
        client.Activate(ref milestoneInt);
        ActionReportFileBuilder builder = new ActionReportFileBuilder(client.GetActionIntelligence());

        GitHubAPIClient.UpdateContentFile(actionEnvironment.OverrideRateLimit(), YouRataConstants.ActionReportMessage, builder.Build(), YouRataConstants.ActionReportFileName, client.LogMessage);
    }
    catch (Exception ex)
    {
        client.SetStatus(MilestoneCondition.MilestoneFailed);
        throw new MilestoneException("ActionReport failed", ex);
    }
    client.SetStatus(MilestoneCondition.MilestoneCompleted);
}
System.Threading.Thread.Sleep(1000);
