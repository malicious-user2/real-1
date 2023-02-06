using static YouRatta.Common.Proto.MilestoneActionIntelligence.Types;

namespace YouRatta.ConflictMonitor.MilestoneData;

internal abstract class BaseMilestoneIntelligence
{
    internal BaseMilestoneIntelligence()
    {
        Condition = MilestoneCondition.MilestonePending;
    }

    internal MilestoneCondition Condition { get; set; }

    internal int ProcessId { get; set; }
}
