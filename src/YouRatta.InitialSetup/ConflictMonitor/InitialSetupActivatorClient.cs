using System;
using YouRatta.Common.Milestone;
using static YouRatta.Common.Proto.MilestoneActionIntelligence.Types;

namespace YouRatta.InitialSetup.ConflictMonitor;

internal class InitialSetupActivatorClient : MilestoneActivatorClient
{
    private static readonly string _milestoneName = "InitialSetup";
    private readonly Type _milestoneType = typeof(InitialSetupActionIntelligence);

    public InitialSetupActivatorClient() : base(_milestoneName)
    {
    }

    public void SetStatus(MilestoneCondition status)
    {
        base.SetStatus(status, _milestoneType, _milestoneName);
    }
}
