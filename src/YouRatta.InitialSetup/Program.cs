using System;
using System.Diagnostics;
using Octokit;
using YouRatta.Common.GitHub;
using YouRatta.Common.Milestone;
using YouRatta.Common.Proto;
using YouRatta.InitialSetup.ConflictMonitor;
using static YouRatta.Common.Proto.MilestoneActionIntelligence.Types;

using (InitialSetupCommunicationClient client = new InitialSetupCommunicationClient())
{
    System.Threading.Thread.Sleep(2000);
    if (client.GetMilestoneActionIntelligence().Condition == MilestoneCondition.MilestoneBlocked) return;
    if (client.GetYouRattaConfiguration().ActionCutOuts.DisableInitialSetupMilestone) return;



    Console.WriteLine(client.GetActionIntelligence());

    ActionIntelligence intel = client.GetActionIntelligence();


    GitHubAPIClient.DeleteSecret(intel.GitHubActionEnvironment, "DEL", client.LogMessage);
    Console.WriteLine(client.GetActionIntelligence());


    System.Console.WriteLine(client.GetYouRattaConfiguration().MilestoneLifetime.MaxRunTime);
    InitialSetupActionIntelligence milestoneActionIntelligence = new InitialSetupActionIntelligence();
    milestoneActionIntelligence.ProcessId = Process.GetCurrentProcess().Id;
    milestoneActionIntelligence.Condition = MilestoneCondition.MilestoneRunning;
    client.SetMilestoneActionIntelligence(milestoneActionIntelligence);
    System.Console.ReadLine();
}

