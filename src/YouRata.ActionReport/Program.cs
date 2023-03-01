using System;
using YouRata.ActionReport.ActionReportFile;
using YouRata.ActionReport.ConflictMonitor;
using YouRata.Common.GitHub;
using static YouRata.Common.Proto.MilestoneActionIntelligence.Types;

using (ActionReportCommunicationClient client = new ActionReportCommunicationClient())
{
    System.Threading.Thread.Sleep(2000);
    if (client.GetMilestoneActionIntelligence().Condition == MilestoneCondition.MilestoneBlocked) return;
    if (client.GetYouRataConfiguration().ActionCutOuts.DisableInitialSetupMilestone) return;


    ActionReportFileBuilder builder = new ActionReportFileBuilder(client.GetActionIntelligence());

    GitHubAPIClient.UpdateContentFile(client.GetActionIntelligence().GitHubActionEnvironment, "update this", builder.Build(), "action-report.json", client.LogMessage);
}
