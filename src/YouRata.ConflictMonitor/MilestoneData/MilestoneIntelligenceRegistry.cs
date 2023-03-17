using System.Collections.Generic;

namespace YouRata.ConflictMonitor.MilestoneData;

internal class MilestoneIntelligenceRegistry
{
    private readonly IReadOnlyCollection<BaseMilestoneIntelligence> _milestones;

    internal MilestoneIntelligenceRegistry()
    {
        InitialSetup = new InitialSetupMilestoneIntelligence();
        YouTubeSync = new YouTubeSyncMilestoneIntelligence();
        ActionReport = new ActionReportMilestoneIntelligence();
        _milestones = new List<BaseMilestoneIntelligence> { InitialSetup, YouTubeSync, ActionReport };
    }

    internal IReadOnlyCollection<BaseMilestoneIntelligence> Milestones => _milestones;

    internal InitialSetupMilestoneIntelligence InitialSetup { get; set; }

    internal YouTubeSyncMilestoneIntelligence YouTubeSync { get; set; }

    internal ActionReportMilestoneIntelligence ActionReport { get; set; }
}
