using System;
using System.Diagnostics;
using System.Threading;
using YouRata.Common;
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
        intelligence = new ActionReportActionIntelligence();
        if (base.IsBlocked(_milestoneType)) return false;
        intelligence = base.Activate<ActionReportActionIntelligence>(_milestoneType, _milestoneName);
        return true;
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

    public void Keepalive()
    {
        base.Keepalive(_milestoneType, _milestoneName);
    }
}
