namespace YouRatta.ConflictMonitor.MilestoneData;

internal class MilestoneIntelligenceRegistry
{
    internal MilestoneIntelligenceRegistry()
    {
        InitialSetup = new InitialSetupMilestoneIntelligence();
    }

    internal InitialSetupMilestoneIntelligence InitialSetup { get; set; }
}
