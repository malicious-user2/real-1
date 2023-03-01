using System;
using YouRata.ActionReport.ActionReportFile;
using YouRata.ActionReport.ConflictMonitor;
using YouRata.Common.GitHub;
using YouRata.Common.Proto;
using static YouRata.Common.Proto.MilestoneActionIntelligence.Types;

using (ActionReportCommunicationClient client = new ActionReportCommunicationClient())
{
    System.Threading.Thread.Sleep(2000);
    if (client.GetMilestoneActionIntelligence().Condition == MilestoneCondition.MilestoneBlocked) return;
    if (client.GetYouRataConfiguration().ActionCutOuts.DisableInitialSetupMilestone) return;

    ActionIntelligence actionInt = client.GetActionIntelligence();
    GitHubActionEnvironment actionEnvironment = actionInt.GitHubActionEnvironment;

    ActionReportFileBuilder builder = new ActionReportFileBuilder(client.GetActionIntelligence());



    GitHubAPIClient.UpdateContentFile(actionEnvironment.GetBlank(), "update this", builder.Build(), "action-report.json", client.LogMessage);
}
