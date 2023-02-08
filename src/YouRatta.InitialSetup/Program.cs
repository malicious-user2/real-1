// See https://aka.ms/new-console-template for more information
using System.Diagnostics;
using YouRatta.Common.Milestone;
using YouRatta.Common.Proto;
using YouRatta.InitialSetup.ConflictMonitor;
using static YouRatta.Common.Proto.MilestoneActionIntelligence.Types;

System.Console.WriteLine("Hello, World!");
using (InitialSetupActivatorClient client = new InitialSetupActivatorClient())
{
    InitialSetupActionIntelligence milestoneActionIntelligence = new InitialSetupActionIntelligence();
    milestoneActionIntelligence.ProcessId = Process.GetCurrentProcess().Id;
    milestoneActionIntelligence.Condition = MilestoneCondition.MilestoneRunning;
    client.SetMilestoneActionIntelligence(milestoneActionIntelligence);
    System.Console.WriteLine(client.GetMilestoneActionIntelligence());
}
System.Console.ReadLine();
