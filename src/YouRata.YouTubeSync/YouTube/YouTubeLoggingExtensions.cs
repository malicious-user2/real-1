using System;
using System.Runtime.CompilerServices;
using YouRata.YouTubeSync.ConflictMonitor;
using static YouRata.Common.Proto.MilestoneActionIntelligence.Types;

namespace YouRata.YouTubeSync.YouTube;

internal static class YouTubeLoggingExtensions
{
    internal static void LogVideoProcessed(this YouTubeSyncCommunicationClient client)
    {
        YouTubeSyncActionIntelligence? milestoneActionIntelligence = client.GetMilestoneActionIntelligence();
        if (milestoneActionIntelligence == null) return;
        milestoneActionIntelligence.VideosProcessed++;
        client.SetMilestoneActionIntelligence(milestoneActionIntelligence);
    }

    internal static void LogVideoSkipped(this YouTubeSyncCommunicationClient client)
    {
        YouTubeSyncActionIntelligence? milestoneActionIntelligence = client.GetMilestoneActionIntelligence();
        if (milestoneActionIntelligence == null) return;
        milestoneActionIntelligence.VideosSkipped++;
        client.SetMilestoneActionIntelligence(milestoneActionIntelligence);
    }

    internal static void LogAPIQueries(this YouTubeSyncCommunicationClient client, YouTubeSyncActionIntelligence intelligence)
    {
        YouTubeSyncActionIntelligence? milestoneActionIntelligence = client.GetMilestoneActionIntelligence();
        if (milestoneActionIntelligence == null) return;
        milestoneActionIntelligence.CalculatedQueriesPerDayRemaining = intelligence.CalculatedQueriesPerDayRemaining;
        milestoneActionIntelligence.LastQueryTime = DateTimeOffset.Now.ToUnixTimeSeconds();
        client.SetMilestoneActionIntelligence(milestoneActionIntelligence);
    }
}
