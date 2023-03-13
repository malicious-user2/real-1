using System;
using Google.Apis.YouTube.v3.Data;
using System.Collections.Generic;
using Google.Protobuf;
using Newtonsoft.Json;
using YouRata.Common;
using YouRata.Common.ActionReport;
using YouRata.Common.Configurations;
using static YouRata.Common.Proto.MilestoneActionIntelligence.Types;
using YouRata.Common.YouTube;
using YouRata.YouTubeSync.ConflictMonitor;

namespace YouRata.YouTubeSync.YouTube;

internal static class YouTubeQuotaHelper
{

    public static bool HasRemainingCalls(YouTubeSyncActionIntelligence intelligence)
    {
        if (intelligence.CalculatedQueriesPerDayRemaining < 100 && intelligence.CalculatedQueriesPerDayRemaining > 0)
        {
            Console.WriteLine($"WARNING: Only {intelligence.CalculatedQueriesPerDayRemaining} YouTube API calls remaining");
        }
        if (intelligence.CalculatedQueriesPerDayRemaining < 4)
        {
            return false;
        }
        return true;
    }

    public static bool HasEnoughCallsForUpdate(YouTubeSyncActionIntelligence intelligence, List<Video> currentVideos)
    {
        int estimatedCostForUpdate = ((currentVideos.Count + 1) * YouTubeConstants.VideosResourceUpdateQuotaCost);
        estimatedCostForUpdate += YouTubeConstants.VideoListQuotaCost;
        if (estimatedCostForUpdate > intelligence.CalculatedQueriesPerDayRemaining)
        {
            return false;
        }
        return true;
    }

    public static void SetPreviousActionReport(YouTubeConfiguration config, YouTubeSyncCommunicationClient client, YouTubeSyncActionIntelligence intelligence, ActionReportLayout previousActionReport)
    {
        if (string.IsNullOrEmpty(previousActionReport.YouTubeSyncIntelligence))
        {
            intelligence.CalculatedQueriesPerDayRemaining = config.QueriesPerDay;
        }
        else
        {
            YouTubeSyncActionIntelligence previousIntelligence = JsonParser.Default.Parse<YouTubeSyncActionIntelligence>(previousActionReport.YouTubeSyncIntelligence);
            TimeZoneInfo resetTimeZone = TimeZoneInfo.FindSystemTimeZoneById(YouRataConstants.PacificTimeZone);
            long resetDateSeconds = ((DateTimeOffset)DateTime.UtcNow.Add(resetTimeZone.BaseUtcOffset).Date).ToUnixTimeSeconds();
            DateTime resetDateTime = DateTimeOffset.FromUnixTimeSeconds(resetDateSeconds).Subtract(resetTimeZone.BaseUtcOffset).DateTime;
            long resetTime = ((DateTimeOffset)resetDateTime).ToUnixTimeSeconds();
            if (previousIntelligence.LastQueryTime == 0 || resetTime > previousIntelligence.LastQueryTime)
            {
                intelligence.CalculatedQueriesPerDayRemaining = config.QueriesPerDay;
            }
            else
            {
                intelligence.CalculatedQueriesPerDayRemaining = previousIntelligence.CalculatedQueriesPerDayRemaining;
            }
            intelligence.FirstVideoPublishTime = previousIntelligence.FirstVideoPublishTime;
            intelligence.OutstandingVideoPublishTime = previousIntelligence.OutstandingVideoPublishTime;
            intelligence.HasOutstandingVideos = previousIntelligence.HasOutstandingVideos;
        }
        Console.WriteLine(intelligence.ToString());
        client.SetMilestoneActionIntelligence(intelligence);
    }
}
