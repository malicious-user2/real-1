using System;
using System.Diagnostics;
using System.Threading;
using YouRata.Common;
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
        int retryCount = 0;
        while (retryCount < 3)
        {
            try
            {
                SetMilestoneActionIntelligence(milestoneActionIntelligence);
                break;
            }
            catch (Grpc.Core.RpcException ex)
            {
                retryCount++;
                if (retryCount > 1)
                {
                    throw new MilestoneException("Failed to connect to ConflictMonitor", ex);
                }
            }
            TimeSpan backOff = APIBackoffHelper.GetRandomBackoff(TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(10));
            Thread.Sleep(backOff);
        }
        intelligence = milestoneActionIntelligence;
        if (intelligence.Condition != MilestoneCondition.MilestoneBlocked)
        {
            Console.WriteLine($"Entering {_milestoneName}");
            return true;
        }
        return false;
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
