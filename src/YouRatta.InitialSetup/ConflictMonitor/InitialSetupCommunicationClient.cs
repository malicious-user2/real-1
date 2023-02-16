using System;
using System.Diagnostics;
using YouRatta.Common.Milestone;
using static YouRatta.Common.Proto.MilestoneActionIntelligence.Types;

namespace YouRatta.InitialSetup.ConflictMonitor;

internal class InitialSetupCommunicationClient : MilestoneCommunicationClient
{
    private static readonly string _milestoneName = "InitialSetup";
    private readonly Type _milestoneType = typeof(InitialSetupActionIntelligence);

    public InitialSetupCommunicationClient() : base()
    {
    }

    public void Activate()
    {
        InitialSetupActionIntelligence milestoneActionIntelligence = new InitialSetupActionIntelligence();
        milestoneActionIntelligence.ProcessId = Process.GetCurrentProcess().Id;
        milestoneActionIntelligence.Condition = MilestoneCondition.MilestoneRunning;
        SetMilestoneActionIntelligence(milestoneActionIntelligence);
        Console.WriteLine($"Entering {_milestoneName}");
    }

    public void SetStatus(MilestoneCondition status)
    {
        base.SetStatus(status, _milestoneType, _milestoneName);
    }

    public MilestoneCondition GetStatus()
    {
        return base.GetStatus(_milestoneType);
    }

    public void SetMilestoneActionIntelligence(InitialSetupActionIntelligence initialSetupActionIntelligence)
    {
        base.SetMilestoneActionIntelligence(initialSetupActionIntelligence, _milestoneType, _milestoneName);
    }

    public InitialSetupActionIntelligence GetMilestoneActionIntelligence()
    {
        InitialSetupActionIntelligence initialSetupActionIntelligence = new InitialSetupActionIntelligence();
        object? milestoneActionIntelligence = base.GetMilestoneActionIntelligence(_milestoneType);
        if (milestoneActionIntelligence != null)
        {
            initialSetupActionIntelligence = (InitialSetupActionIntelligence)milestoneActionIntelligence;
        }
        return initialSetupActionIntelligence;
    }

    public void LogMessage(string message)
    {
        base.LogMessage(message, _milestoneName);
    }
}
