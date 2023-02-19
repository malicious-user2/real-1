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
        YouTubeSync = new YouTubeSyncMilestoneIntelligence();
        _milestones = new List<BaseMilestoneIntelligence>() { InitialSetup, YouTubeSync };
    }

    internal IReadOnlyCollection<BaseMilestoneIntelligence> Milestones => _milestones;

    internal InitialSetupMilestoneIntelligence InitialSetup { get; set; }

    internal YouTubeSyncMilestoneIntelligence YouTubeSync { get; set; }
}
