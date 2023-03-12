public static List<Video> GetRecentChannelVideos(YouTubeConfiguration config, out long firstPublishTime, out long lastPublishTime, List<ResourceId> excludeVideos, YouTubeSyncActionIntelligence intelligence, YouTubeService service, YouTubeSyncCommunicationClient client)
{
    firstPublishTime = intelligence.FirstVideoPublishTime;
    lastPublishTime = 0;
    if (!YouTubeQuotaHelper.HasRemainingCalls(intelligence)) throw new MilestoneException("YouTube API rate limit exceeded");
    List<Video> channelVideos = new List<Video>();
    SearchResource.ListRequest searchRequest = new SearchResource.ListRequest(service, new string[] { YouTubeConstants.RequestSnippetPart });
    if (string.IsNullOrEmpty(config.ChannelId)) return channelVideos;
    searchRequest.ChannelId = config.ChannelId;
    searchRequest.MaxResults = 50;
    if (config.UseForMine)
    {
        searchRequest.ForMine = true;
    }
    else
    {
        searchRequest.VideoDuration = GetVideoDurationFromConfig(config);
    }
    searchRequest.Type = YouTubeConstants.VideoType;
    searchRequest.Order = SearchResource.ListRequest.OrderEnum.Date;
    searchRequest.PublishedAfter = Utilities.GetStringFromDateTime(DateTimeOffset.FromUnixTimeSeconds(firstPublishTime).DateTime);
    Console.WriteLine(searchRequest.PublishedAfter);
    bool requestNextPage = true;
    while (requestNextPage)
    {
        Func<SearchListResponse> getSearchResponse = (() =>
        {
            return searchRequest.Execute();
        });
        SearchListResponse? searchResponse = YouTubeRetryHelper.RetryCommand(intelligence, YouTubeConstants.SearchListQuotaCost, getSearchResponse, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(10), client.LogMessage);
        if (searchResponse == null) throw new MilestoneException($"Could not get YouTube video list for channel id {searchRequest.ChannelId}");
        foreach (SearchResult searchResult in searchResponse.Items)
        {
            if (!YouTubeQuotaHelper.HasEnoughCallsForUpdate(intelligence, channelVideos)) return channelVideos;
            Console.WriteLine(intelligence.CalculatedQueriesPerDayRemaining);
            if (searchResult.Snippet.PublishedAt == null) continue;
            if (searchResult.Id.Kind != YouTubeConstants.VideoKind) continue;
            VideosResource.ListRequest videoRequest = new VideosResource.ListRequest(service, new string[] { YouTubeConstants.RequestContentDetailsPart, YouTubeConstants.RequestSnippetPart });
            videoRequest.Id = searchResult.Id.VideoId;
            videoRequest.MaxResults = 1;
            Func<VideoListResponse> getVideoResponse = (() =>
            {
                return videoRequest.Execute();
            });
            VideoListResponse? videoResponse = YouTubeRetryHelper.RetryCommand(intelligence, YouTubeConstants.VideoListQuotaCost, getVideoResponse, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(10), client.LogMessage);
            if (videoResponse == null) throw new MilestoneException($"Could not get YouTube video {videoRequest.Id}");
            Video videoDetails = videoResponse.Items.First();
            if (excludeVideos != null && excludeVideos.Find(resourceId => resourceId.VideoId == videoDetails.Id) != null)
            {
                client.LogVideoSkipped();
                continue;
            }
            TimeSpan contentDuration = XmlConvert.ToTimeSpan(videoDetails.ContentDetails.Duration);
            if (contentDuration.TotalSeconds < config.MinVideoSeconds)
            {
                client.LogVideoSkipped();
                continue;
            }
            channelVideos.Add(videoDetails);
            DateTimeOffset publishTimeOffset = new DateTimeOffset(searchResult.Snippet.PublishedAt.Value);
            if (firstPublishTime == 0)
            {
                firstPublishTime = publishTimeOffset.ToUnixTimeSeconds();
            }
            lastPublishTime = publishTimeOffset.ToUnixTimeSeconds();
        }
        requestNextPage = !string.IsNullOrEmpty(searchResponse.NextPageToken);
        searchRequest.PageToken = searchResponse.NextPageToken;
    }
    return channelVideos;
}
