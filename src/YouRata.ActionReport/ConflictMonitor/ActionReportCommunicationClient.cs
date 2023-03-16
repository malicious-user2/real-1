using System;
using System.Diagnostics;
using YouRata.Common.Milestone;
using static YouRata.Common.Proto.MilestoneActionIntelligence.Types;

namespace YouRata.ActionReport.ConflictMonitor;

internal class ActionReportCommunicationClient : MilestoneCommunicationClient
{
    private static readonly string _milestoneName = "ActionReport";
    private readonly Type _milestoneType = typeof(ActionReportActionIntelligence);

    public ActionReportCommunicationClient() : base()
    {
    }

    public void Activate(ref ActionReportActionIntelligence intelligence)
    {
        ActionReportActionIntelligence milestoneActionIntelligence = new ActionReportActionIntelligence
        {
            ProcessId = Process.GetCurrentProcess().Id,
            Condition = MilestoneCondition.MilestoneRunning
        };
        SetMilestoneActionIntelligence(milestoneActionIntelligence);
        Console.WriteLine($"Entering {_milestoneName}");
        intelligence = milestoneActionIntelligence;
    }

    public void SetStatus(MilestoneCondition status)
    {
        base.SetStatus(status, _milestoneType, _milestoneName);
    }

    public MilestoneCondition GetStatus()
    {
        return base.GetStatus(_milestoneType);
    }

    public void SetMilestoneActionIntelligence(ActionReportActionIntelligence initialSetupActionIntelligence)
    {
        base.SetMilestoneActionIntelligence(initialSetupActionIntelligence, _milestoneType, _milestoneName);
    }

    public ActionReportActionIntelligence? GetMilestoneActionIntelligence()
    {
        ActionReportActionIntelligence? initialSetupActionIntelligence = null;
        object? milestoneActionIntelligence = base.GetMilestoneActionIntelligence(_milestoneType);
        if (milestoneActionIntelligence != null)
        {
            initialSetupActionIntelligence = (ActionReportActionIntelligence?)milestoneActionIntelligence;
        }
        return initialSetupActionIntelligence;
    }

    public void LogMessage(string message)
    {
        base.LogMessage(message, _milestoneName);
    }
}
