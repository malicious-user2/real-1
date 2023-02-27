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
}
