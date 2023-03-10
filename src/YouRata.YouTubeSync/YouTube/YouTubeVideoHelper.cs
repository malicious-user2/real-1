using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Reflection.PortableExecutable;
using System.Xml;
using Google.Apis.Util;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using YouRata.Common.Milestone;
using YouRata.Common.YouTube;
using YouRata.YouTubeSync.ConflictMonitor;
using YouRata.YouTubeSync.ErrataBulletin;
using static YouRata.Common.Proto.MilestoneActionIntelligence.Types;

namespace YouRata.YouTubeSync.YouTube;

internal static class YouTubeVideoHelper
{
    public static void UpdateVideoDescription(Video video, string description, YouTubeSyncActionIntelligence intelligence, YouTubeService service, YouTubeSyncCommunicationClient client)
    {
        if (!YouTubeQuotaHelper.HasRemainingCalls(intelligence)) throw new MilestoneException("YouTube API rate limit exceeded");
        if (video.Snippet == null) return;
        foreach (PropertyInfo videoProperty in video.GetType().GetProperties())
        {
            if (videoProperty != null && videoProperty.CanWrite && videoProperty.PropertyType.IsClass)
            {
                if (!videoProperty.Name.Equals("Id") && !videoProperty.Name.Equals("Snippet"))
                {
                    videoProperty.SetValue(video, null);
                }
            }
        }
        video.Snippet.Description = description;
        VideosResource.UpdateRequest videoUpdateRequest = new VideosResource.UpdateRequest(service, video, new string[] { YouTubeConstants.RequestSnippetPart });
        Action videoUpdate = (() =>
        {
            videoUpdateRequest.Execute();
        });
        bool forbidden = false;
        YouTubeRetryHelper.RetryCommand(intelligence, YouTubeConstants.VideosResourceUpdateQuotaCost, videoUpdate, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(10), client.LogMessage, HttpStatusCode.Forbidden, out forbidden);
        if (forbidden) throw new MilestoneException("YouTube API update video forbidden");
    }

    public static List<Video> GetOutstandingChannelVideos(string channelId, out long lastOutstandingPublishTime, List<ResourceId> excludeVideos, YouTubeSyncActionIntelligence intelligence, YouTubeService service, YouTubeSyncCommunicationClient client)
    {
        long firstOutstandingPublishTime = intelligence.OutstandingVideoPublishTime;
        lastOutstandingPublishTime = 0;
        if (!YouTubeQuotaHelper.HasRemainingCalls(intelligence)) throw new MilestoneException("YouTube API rate limit exceeded");
        List<Video> channelVideos = new List<Video>();
        SearchResource.ListRequest searchRequest = new SearchResource.ListRequest(service, new string[] { YouTubeConstants.RequestSnippetPart });
        if (string.IsNullOrEmpty(channelId)) return channelVideos;
        searchRequest.ChannelId = channelId;
        searchRequest.MaxResults = 50;
        searchRequest.Order = SearchResource.ListRequest.OrderEnum.Date;
        searchRequest.PublishedBefore = Utilities.GetStringFromDateTime(DateTimeOffset.FromUnixTimeSeconds(firstOutstandingPublishTime - 1).DateTime);
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
                channelVideos.Add(videoDetails);
                DateTimeOffset publishTimeOffset = new DateTimeOffset(searchResult.Snippet.PublishedAt.Value);
                if (firstOutstandingPublishTime == 0)
                {
                    firstOutstandingPublishTime = publishTimeOffset.ToUnixTimeSeconds();
                }
                lastOutstandingPublishTime = publishTimeOffset.ToUnixTimeSeconds();
            }
            requestNextPage = !string.IsNullOrEmpty(searchResponse.NextPageToken);
            searchRequest.PageToken = searchResponse.NextPageToken;
        }
        return channelVideos;
    }

    public static List<Video> GetRecentChannelVideos(string channelId, out long firstPublishTime, out long lastPublishTime, List<ResourceId> excludeVideos, YouTubeSyncActionIntelligence intelligence, YouTubeService service, YouTubeSyncCommunicationClient client)
    {
        firstPublishTime = intelligence.FirstVideoPublishTime;
        lastPublishTime = 0;
        if (!YouTubeQuotaHelper.HasRemainingCalls(intelligence)) throw new MilestoneException("YouTube API rate limit exceeded");
        List<Video> channelVideos = new List<Video>();
        SearchResource.ListRequest searchRequest = new SearchResource.ListRequest(service, new string[] { YouTubeConstants.RequestSnippetPart });
        if (string.IsNullOrEmpty(channelId)) return channelVideos;
        searchRequest.ChannelId = channelId;
        searchRequest.MaxResults = 50;
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
}
