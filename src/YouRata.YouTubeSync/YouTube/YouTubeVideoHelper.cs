using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Reflection.PortableExecutable;
using System.Xml;
using Google.Apis.Util;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using Microsoft.Extensions.Configuration;
using YouRata.Common.Configurations;
using YouRata.Common.Milestone;
using YouRata.Common.YouTube;
using YouRata.YouTubeSync.ConflictMonitor;
using YouRata.YouTubeSync.ErrataBulletin;
using static Google.Apis.YouTube.v3.SearchResource.ListRequest;
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

    public static VideoDurationEnum GetVideoDurationFromConfig(YouTubeConfiguration config)
    {
        switch (config.VideoDurationFilter)
        {
            case "any":
                return VideoDurationEnum.Any;
            case "long":
                return VideoDurationEnum.Long__;
            case "medium":
                return VideoDurationEnum.Medium;
            case "short":
                return VideoDurationEnum.Short__;
        }
        return VideoDurationEnum.Any;
    }

    internal static List<Video> GetChannelVideos(YouTubeConfiguration config, long? publishedBefore, long? publishedAfter, out long? lastPublishTime, List<ResourceId> excludeVideos, YouTubeSyncActionIntelligence intelligence, YouTubeService service, YouTubeSyncCommunicationClient client)
    {
        List<Video> channelVideos = new List<Video>();
        lastPublishTime = null;
        if (!YouTubeQuotaHelper.HasRemainingCalls(intelligence)) throw new MilestoneException("YouTube API rate limit exceeded");
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
        searchRequest.Order = OrderEnum.Date;
        if (publishedBefore != null)
        {
            searchRequest.PublishedBefore = Utilities.GetStringFromDateTime(DateTimeOffset.FromUnixTimeSeconds(publishedBefore.Value).DateTime);
        }
        if (publishedAfter != null)
        {
            searchRequest.PublishedAfter = Utilities.GetStringFromDateTime(DateTimeOffset.FromUnixTimeSeconds(publishedAfter.Value).DateTime);
        }
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
                    intelligence.VideosSkipped++;
                    continue;
                }
                TimeSpan contentDuration = XmlConvert.ToTimeSpan(videoDetails.ContentDetails.Duration);
                if (contentDuration.TotalSeconds < config.MinVideoSeconds)
                {
                    intelligence.VideosSkipped++;
                    continue;
                }
                channelVideos.Add(videoDetails);
                DateTimeOffset publishTimeOffset = new DateTimeOffset(searchResult.Snippet.PublishedAt.Value);
                lastPublishTime = publishTimeOffset.ToUnixTimeSeconds();
            }
            requestNextPage = !string.IsNullOrEmpty(searchResponse.NextPageToken);
            searchRequest.PageToken = searchResponse.NextPageToken;
        }
        return channelVideos;
    }

    public static List<Video> GetOutstandingChannelVideos(YouTubeConfiguration config, List<ResourceId> excludeVideos, YouTubeSyncActionIntelligence intelligence, YouTubeService service, YouTubeSyncCommunicationClient client)
    {
        long firstOutstandingPublishTime = intelligence.OutstandingVideoPublishTime;
        long? lastPublishTime;
        List<Video> outstandingChannelVideos = GetChannelVideos(config, firstOutstandingPublishTime, null, out lastPublishTime, excludeVideos, intelligence, service, client);
        if (lastPublishTime.HasValue)
        {
            intelligence.OutstandingVideoPublishTime = lastPublishTime.Value;
        }
        else
        {
            intelligence.OutstandingVideoPublishTime = 0;
        }
        return outstandingChannelVideos;
    }

    public static List<Video> GetRecentChannelVideos(YouTubeConfiguration config, List<ResourceId> excludeVideos, YouTubeSyncActionIntelligence intelligence, YouTubeService service, YouTubeSyncCommunicationClient client)
    {
        long firstRecentPublishTime = intelligence.FirstVideoPublishTime;
        long lastRecentPublishTime;
        long? lastPublishTime;
        List<Video> recentChannelVideos = GetChannelVideos(config, null, firstRecentPublishTime, out lastPublishTime, excludeVideos, intelligence, service, client);
        if (lastPublishTime.HasValue)
        {
            lastRecentPublishTime = lastPublishTime.Value;
        }
        else
        {
            lastRecentPublishTime = 0;
        }
        if (firstRecentPublishTime == 0 || recentChannelVideos.Count > 0)
        {
            DateTime? firstPublishDateTime = recentChannelVideos.First().Snippet.PublishedAt;
            if (firstPublishDateTime.HasValue)
            {
                DateTimeOffset publishTimeOffset = new DateTimeOffset(firstPublishDateTime.Value);
                firstRecentPublishTime = publishTimeOffset.ToUnixTimeSeconds();
                intelligence.OutstandingVideoPublishTime = lastRecentPublishTime;
            }
        }
        intelligence.FirstVideoPublishTime = firstRecentPublishTime;
        return recentChannelVideos;
    }
}
