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

    public bool Activate(out ActionReportActionIntelligence intelligence)
    {
        ActionReportActionIntelligence milestoneActionIntelligence = new ActionReportActionIntelligence
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
