using System;
using System.Diagnostics;
using YouRata.Common.Milestone;
using static YouRata.Common.Proto.MilestoneActionIntelligence.Types;

namespace YouRata.InitialSetup.ConflictMonitor;

internal class InitialSetupCommunicationClient : MilestoneCommunicationClient
{
    private static readonly string _milestoneName = "InitialSetup";
    private readonly Type _milestoneType = typeof(InitialSetupActionIntelligence);

    public InitialSetupCommunicationClient() : base()
    {
    }

    public bool Activate(out InitialSetupActionIntelligence intelligence)
    {
        InitialSetupActionIntelligence milestoneActionIntelligence = new InitialSetupActionIntelligence
        {
            ProcessId = Process.GetCurrentProcess().Id,
            Condition = MilestoneCondition.MilestoneRunning
        };
        SetMilestoneActionIntelligence(milestoneActionIntelligence);
        intelligence = milestoneActionIntelligence;
        if (intelligence.Condition != MilestoneCondition.MilestoneBlocked)
        {
            Console.WriteLine($"Entering {_milestoneName}");
            return true;
        }
        else
        {
            return false;
        }
    }

    public void SetStatus(MilestoneCondition status)
    {
        base.SetStatus(status, _milestoneType, _milestoneName);
    }

    public void SetMilestoneActionIntelligence(InitialSetupActionIntelligence initialSetupActionIntelligence)
    {
        base.SetMilestoneActionIntelligence(initialSetupActionIntelligence, _milestoneType, _milestoneName);
    }

    public InitialSetupActionIntelligence? GetMilestoneActionIntelligence()
    {
        InitialSetupActionIntelligence? initialSetupActionIntelligence = null;
        object? milestoneActionIntelligence = base.GetMilestoneActionIntelligence(_milestoneType);
        if (milestoneActionIntelligence != null)
        {
            initialSetupActionIntelligence = (InitialSetupActionIntelligence?)milestoneActionIntelligence;
        }
        return initialSetupActionIntelligence;
    }

    public void LogMessage(string message)
    {
        base.LogMessage(message, _milestoneName);
    }
}
