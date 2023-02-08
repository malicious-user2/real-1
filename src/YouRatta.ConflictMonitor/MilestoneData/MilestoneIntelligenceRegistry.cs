using System.Collections.Generic;
using Microsoft.Extensions.Configuration.Ini;
using static YouRatta.Common.Proto.MilestoneActionIntelligence.Types;

namespace YouRatta.ConflictMonitor.MilestoneData;

internal class MilestoneIntelligenceRegistry
{
    private readonly IReadOnlyCollection<BaseMilestoneIntelligence> _milestones;

    internal MilestoneIntelligenceRegistry()
    {
        InitialSetup = new InitialSetupMilestoneIntelligence();
        InitialSetup.Condition = MilestoneCondition.MilestonePending;
        _milestones = new List<BaseMilestoneIntelligence>() { InitialSetup };
    }

    internal IReadOnlyCollection<BaseMilestoneIntelligence> Milestones => _milestones;

    internal InitialSetupMilestoneIntelligence InitialSetup { get; set; }
}
