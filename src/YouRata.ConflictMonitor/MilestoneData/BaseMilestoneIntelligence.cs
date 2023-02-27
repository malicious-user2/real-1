using static YouRata.Common.Proto.MilestoneActionIntelligence.Types;

namespace YouRata.ConflictMonitor.MilestoneData;

internal abstract class BaseMilestoneIntelligence
{
    internal BaseMilestoneIntelligence()
    {
        Condition = MilestoneCondition.MilestonePending;
    }

    internal MilestoneCondition Condition { get; set; }

    internal int ProcessId { get; set; }

    internal long StartTime { get; set; }

    internal long LastUpdate { get; set; }
}
