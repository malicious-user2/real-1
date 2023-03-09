namespace YouRata.ConflictMonitor.MilestoneData;

internal class YouTubeSyncMilestoneIntelligence : BaseMilestoneIntelligence
{
    public YouTubeSyncMilestoneIntelligence()
    {
        VideosProcessed = 0;
        VideosSkipped = 0;
    }

    public int VideosProcessed { get; set; }

    public int VideosSkipped { get; set; }

    public int CalculatedQueriesPerDayRemaining { get; set; }

    public long LastQueryTime { get; set; }

    public long FirstVideoPublishTime { get; set; }

    public long OutstandingVideoPublishTime { get; set; }

    public bool HasOutstandingVideos { get; set; }
}
