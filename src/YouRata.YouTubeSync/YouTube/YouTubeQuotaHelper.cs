using System;
using Google.Protobuf;
using Newtonsoft.Json;
using YouRata.Common;
using YouRata.Common.ActionReport;
using YouRata.Common.Configurations;
using static YouRata.Common.Proto.MilestoneActionIntelligence.Types;

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

    public static void FillPreviousActionReport(YouTubeConfiguration config, YouTubeSyncActionIntelligence intelligence, ActionReportLayout previousActionReport)
    {
        if (string.IsNullOrEmpty(previousActionReport.YouTubeSyncIntelligence))
        {
            intelligence.CalculatedQueriesPerDayRemaining = config.QueriesPerDay;
        }
        else
        {
            YouTubeSyncActionIntelligence previousIntelligence = JsonParser.Default.Parse<YouTubeSyncActionIntelligence>(previousActionReport.YouTubeSyncIntelligence);
            
            DateTime lastQuery = DateTimeOffset.FromUnixTimeSeconds(previousIntelligence.LastQueryTime).Date;
            DateTime resetTime = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.UtcNow, TimeZoneInfo.Utc.Id, YouRataConstants.PacificTimeZone).Date;
            Console.WriteLine($"old: {lastQuery} reset: {resetTime}");
            if (previousIntelligence.LastQueryTime == 0 || resetTime > lastQuery)
            {
                intelligence.CalculatedQueriesPerDayRemaining = config.QueriesPerDay;
            }
            else
            {
                intelligence.CalculatedQueriesPerDayRemaining = previousIntelligence.CalculatedQueriesPerDayRemaining;
            }
            intelligence.LastVideoPublishTime = previousIntelligence.LastVideoPublishTime;
        }
    }
}
