// See https://aka.ms/new-console-template for more information
using YouRatta.Common.Milestone;
using YouRatta.InitialSetup.ConflictMonitor;
using static YouRatta.Common.Proto.MilestoneActionIntelligence.Types;

System.Console.WriteLine("Hello, World!");
using (InitialSetupActivatorClient client = new InitialSetupActivatorClient())
{
    client.SetStatus(YouRatta.Common.Proto.MilestoneActionIntelligence.Types.MilestoneCondition.MilestoneRunning);
    System.Console.WriteLine(client.GetStatus(typeof(InitialSetupActionIntelligence)));
    System.Console.WriteLine(client.GetInitialSetupActionIntelligence());
}
System.Console.ReadLine();
