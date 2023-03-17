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

    public bool Activate(out YouTubeSyncActionIntelligence intelligence)
    {
        YouTubeSyncActionIntelligence milestoneActionIntelligence = new YouTubeSyncActionIntelligence
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
