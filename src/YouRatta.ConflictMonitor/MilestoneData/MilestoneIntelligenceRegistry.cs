using System.Collections.Generic;
using Microsoft.Extensions.Configuration.Ini;

namespace YouRatta.ConflictMonitor.MilestoneData;

internal class MilestoneIntelligenceRegistry
{
    private readonly IReadOnlyCollection<BaseMilestoneIntelligence> _milestones;

    internal MilestoneIntelligenceRegistry()
    {
        InitialSetup = new InitialSetupMilestoneIntelligence();
        _milestones = new List<BaseMilestoneIntelligence>() { InitialSetup };
    }

    internal IReadOnlyCollection<BaseMilestoneIntelligence> Milestones => _milestones;

    internal InitialSetupMilestoneIntelligence InitialSetup { get; set; }
}
