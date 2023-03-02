using System;
using System.Runtime.CompilerServices;
using YouRata.YouTubeSync.ConflictMonitor;
using static YouRata.Common.Proto.MilestoneActionIntelligence.Types;

namespace YouRata.YouTubeSync.YouTube;

internal static class YouTubeLoggingExtensions
{
    internal static void LogVideosProcessed(this YouTubeSyncCommunicationClient client)
    {
        YouTubeSyncActionIntelligence? milestoneActionIntelligence = client.GetMilestoneActionIntelligence();
        if (milestoneActionIntelligence == null) return;
        milestoneActionIntelligence.VideosProcessed++;
        client.SetMilestoneActionIntelligence(milestoneActionIntelligence);
    }

    internal static void LogVideosSkipped(this YouTubeSyncCommunicationClient client)
    {
        YouTubeSyncActionIntelligence? milestoneActionIntelligence = client.GetMilestoneActionIntelligence();
        if (milestoneActionIntelligence == null) return;
        milestoneActionIntelligence.VideosSkipped++;
        client.SetMilestoneActionIntelligence(milestoneActionIntelligence);
    }
}
