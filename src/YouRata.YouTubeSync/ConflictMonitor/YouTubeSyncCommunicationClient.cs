using System;
using System.Diagnostics;
using YouRata.Common.Milestone;
using static YouRata.Common.Proto.MilestoneActionIntelligence.Types;

namespace YouRata.YouTubeSync.ConflictMonitor;

internal class YouTubeSyncCommunicationClient : MilestoneCommunicationClient
{
    private static readonly string _milestoneName = "YouTubeSync";
    private readonly Type _milestoneType = typeof(YouTubeSyncActionIntelligence);

    public YouTubeSyncCommunicationClient() : base()
    {
    }

    public void Activate()
    {
        YouTubeSyncActionIntelligence milestoneActionIntelligence = new YouTubeSyncActionIntelligence();
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

    public void SetMilestoneActionIntelligence(YouTubeSyncActionIntelligence youTubeSyncActionIntelligence)
    {
        base.SetMilestoneActionIntelligence(youTubeSyncActionIntelligence, _milestoneType, _milestoneName);
    }

    public YouTubeSyncActionIntelligence? GetMilestoneActionIntelligence()
    {
        YouTubeSyncActionIntelligence? youTubeSyncActionIntelligence = null;
        object? milestoneActionIntelligence = base.GetMilestoneActionIntelligence(_milestoneType);
        if (milestoneActionIntelligence != null)
        {
            youTubeSyncActionIntelligence = (YouTubeSyncActionIntelligence?)milestoneActionIntelligence;
        }
        return youTubeSyncActionIntelligence;
    }

    public void LogMessage(string message)
    {
        base.LogMessage(message, _milestoneName);
    }
}
