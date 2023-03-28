// Copyright (c) 2023 battleship-systems.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Xml;
using Google.Apis.Util;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using YouRata.Common.Configuration.YouTube;
using YouRata.Common.Milestone;
using YouRata.Common.YouTube;
using YouRata.YouTubeSync.ConflictMonitor;
using static Google.Apis.YouTube.v3.SearchResource.ListRequest;
using static YouRata.Common.Proto.MilestoneActionIntelligence.Types;

namespace YouRata.YouTubeSync.YouTube;

/// <summary>
/// YouTube video action helper class
/// </summary>
internal static class YouTubeVideoHelper
{
    /// <summary>
    /// Get a list of videos published before the first outstanding video time
    /// </summary>
    /// <param name="config"></param>
    /// <param name="excludeVideos"></param>
    /// <param name="intelligence"></param>
    /// <param name="service"></param>
    /// <param name="client"></param>
    /// <returns></returns>
    public static List<Video> GetOutstandingChannelVideos(YouTubeConfiguration config, List<ResourceId> excludeVideos,
        YouTubeSyncActionIntelligence intelligence, YouTubeService service, YouTubeSyncCommunicationClient client)
    {
        long firstOutstandingPublishTime = (intelligence.OutstandingVideoPublishTime - 1);
        long? lastPublishTime;
        List<Video> outstandingChannelVideos = GetChannelVideos(config, firstOutstandingPublishTime, null, out lastPublishTime,
            excludeVideos, intelligence, service, client);
        if (lastPublishTime.HasValue)
        {
            // Save the last publish time to the milestone intelligence
            intelligence.OutstandingVideoPublishTime = lastPublishTime.Value;
        }
        else
        {
            // No outstanding videos exist
            intelligence.OutstandingVideoPublishTime = 0;
        }

        return outstandingChannelVideos;
    }

    /// <summary>
    /// Get a list of videos published after the first video time
    /// </summary>
    /// <param name="config"></param>
    /// <param name="excludeVideos"></param>
    /// <param name="intelligence"></param>
    /// <param name="service"></param>
    /// <param name="client"></param>
    /// <returns></returns>
    public static List<Video> GetRecentChannelVideos(YouTubeConfiguration config, List<ResourceId> excludeVideos,
        YouTubeSyncActionIntelligence intelligence, YouTubeService service, YouTubeSyncCommunicationClient client)
    {
        long firstRecentPublishTime = intelligence.FirstVideoPublishTime;
        long lastRecentPublishTime;
        List<Video> recentChannelVideos = GetChannelVideos(config, null, firstRecentPublishTime, out long? lastPublishTime, excludeVideos,
            intelligence, service, client);
        if (lastPublishTime.HasValue)
        {
            lastRecentPublishTime = lastPublishTime.Value;
        }
        else
        {
            lastRecentPublishTime = 0;
        }

        if (firstRecentPublishTime == 0 && recentChannelVideos.Count > 0)
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

    public static void UpdateVideoDescription(Video video, string description, YouTubeSyncActionIntelligence intelligence,
        YouTubeService service, YouTubeSyncCommunicationClient client)
    {
        if (!YouTubeQuotaHelper.HasRemainingCalls(intelligence)) throw new MilestoneException("YouTube API rate limit exceeded");
        if (video.Snippet == null) return;
        foreach (PropertyInfo videoProperty in video.GetType().GetProperties())
        {
            if (videoProperty.CanWrite && videoProperty.PropertyType.IsClass)
            {
                if (!videoProperty.Name.Equals("Id") && !videoProperty.Name.Equals("Snippet"))
                {
                    videoProperty.SetValue(video, null);
                }
            }
        }

        video.Snippet.Description = description;
        VideosResource.UpdateRequest videoUpdateRequest =
            new VideosResource.UpdateRequest(service, video, new[] { YouTubeConstants.RequestSnippetPart });
        Action videoUpdate = (() => { videoUpdateRequest.Execute(); });
        YouTubeRetryHelper.RetryCommand(intelligence, YouTubeConstants.VideosResourceUpdateQuotaCost, videoUpdate, TimeSpan.FromSeconds(5),
            TimeSpan.FromSeconds(10), client.LogMessage, HttpStatusCode.Forbidden, out bool forbidden);
        if (forbidden) throw new MilestoneException("YouTube API update video forbidden");
    }

    internal static List<Video> GetChannelVideos(YouTubeConfiguration config, long? publishedBefore, long? publishedAfter,
        out long? lastPublishTime, List<ResourceId> excludeVideos, YouTubeSyncActionIntelligence intelligence, YouTubeService service,
        YouTubeSyncCommunicationClient client)
    {
        List<Video> channelVideos = new List<Video>();
        lastPublishTime = null;
        if (!YouTubeQuotaHelper.HasRemainingCalls(intelligence)) throw new MilestoneException("YouTube API rate limit exceeded");
        SearchResource.ListRequest searchRequest = new SearchResource.ListRequest(service, new[] { YouTubeConstants.RequestSnippetPart });
        if (string.IsNullOrEmpty(config.ChannelId)) return channelVideos;
        searchRequest.ChannelId = config.ChannelId;
        searchRequest.MaxResults = 50;
        searchRequest.VideoDuration = GetVideoDurationFromConfig(config);
        searchRequest.Type = YouTubeConstants.VideoType;
        searchRequest.Order = OrderEnum.Date;
        if (publishedBefore != null)
        {
            searchRequest.PublishedBefore =
                Utilities.GetStringFromDateTime(DateTimeOffset.FromUnixTimeSeconds(publishedBefore.Value).DateTime);
        }

        if (publishedAfter != null)
        {
            searchRequest.PublishedAfter =
                Utilities.GetStringFromDateTime(DateTimeOffset.FromUnixTimeSeconds(publishedAfter.Value).DateTime);
        }

        bool requestNextPage = true;
        while (requestNextPage)
        {
            Func<SearchListResponse> getSearchResponse = (() => { return searchRequest.Execute(); });
            SearchListResponse? searchResponse = YouTubeRetryHelper.RetryCommand(intelligence, YouTubeConstants.SearchListQuotaCost,
                getSearchResponse, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(10), client.LogMessage);
            client.Keepalive();
            if (searchResponse == null)
                throw new MilestoneException($"Could not get YouTube video list for channel id {searchRequest.ChannelId}");
            foreach (SearchResult searchResult in searchResponse.Items)
            {
                if (!YouTubeQuotaHelper.HasEnoughCallsForUpdate(intelligence, channelVideos)) return channelVideos;
                if (searchResult.Snippet.PublishedAt == null) continue;
                if (searchResult.Id.Kind != YouTubeConstants.VideoKind) continue;
                if (config.ExcludeVideos?.Contains(searchResult.Id.VideoId) == true) continue;
                VideosResource.ListRequest videoRequest = new VideosResource.ListRequest(service,
                    new[] { YouTubeConstants.RequestContentDetailsPart, YouTubeConstants.RequestSnippetPart });
                videoRequest.Id = searchResult.Id.VideoId;
                videoRequest.MaxResults = 1;
                Func<VideoListResponse> getVideoResponse = (() => { return videoRequest.Execute(); });
                VideoListResponse? videoResponse = YouTubeRetryHelper.RetryCommand(intelligence, YouTubeConstants.VideoListQuotaCost,
                    getVideoResponse, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(10), client.LogMessage);
                client.Keepalive();
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
}
