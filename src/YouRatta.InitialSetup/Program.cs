// See https://aka.ms/new-console-template for more information
using YouRatta.Common.Milestone;
using YouRatta.InitialSetup.ConflictMonitor;
using static YouRatta.Common.Proto.MilestoneActionIntelligence.Types;

System.Console.WriteLine("Hello, World!");
using (InitialSetupActivatorClient client = new InitialSetupActivatorClient())
{
    client.SetStatus(YouRatta.Common.Proto.MilestoneActionIntelligence.Types.MilestoneCondition.MilestoneRunning);
    client.GetStatus(typeof(InitialSetupActionIntelligence));
}
