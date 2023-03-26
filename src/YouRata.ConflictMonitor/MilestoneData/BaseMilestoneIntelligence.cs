// Copyright (c) 2023 battleship-systems.
// Licensed under the MIT license.

using System;
using static YouRata.Common.Proto.MilestoneActionIntelligence.Types;

namespace YouRata.ConflictMonitor.MilestoneData;

internal abstract class BaseMilestoneIntelligence
{
    private protected BaseMilestoneIntelligence()
    {
        Condition = MilestoneCondition.MilestonePending;
    }

    internal MilestoneCondition Condition { get; set; }

    internal long LastUpdate { get; set; }
    internal int ProcessId { get; set; }

    internal long StartTime { get; set; }
}
