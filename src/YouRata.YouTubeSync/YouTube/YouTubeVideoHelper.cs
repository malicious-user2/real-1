using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Xml;
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
        video.ContentDetails = null;
        video.FileDetails = null;
        video.LiveStreamingDetails = null;
        video.Localizations = null;
        video.Player = null;
        video.ProcessingDetails = null;
        video.RecordingDetails = null;
        video.Statistics = null;
        video.Status = null;
        video.Suggestions = null;
        video.TopicDetails = null;
        if (video.Snippet == null) return;
        video.Snippet.Description = description;
        video.ContentDetails = null;
        VideosResource.UpdateRequest videoUpdateRequest = new VideosResource.UpdateRequest(service, video, new string[] { YouTubeConstants.RequestSnippetPart });
        Action videoUpdate = (() =>
        {
            videoUpdateRequest.Execute();
        });
        bool forbidden = false;
        YouTubeRetryHelper.RetryCommand(intelligence, 50, videoUpdate, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(10), client.LogMessage, HttpStatusCode.Forbidden, out forbidden);
        if (forbidden) throw new MilestoneException("YouTube API update video forbidden");
    }

    public static List<Video> GetChannelVideos(string channelId, List<ResourceId> excludeVideos, YouTubeSyncActionIntelligence intelligence, YouTubeService service, YouTubeSyncCommunicationClient client)
    {
        if (!YouTubeQuotaHelper.HasRemainingCalls(intelligence)) throw new MilestoneException("YouTube API rate limit exceeded");
        List<Video> channelVideos = new List<Video>();
        SearchResource.ListRequest searchRequest = new SearchResource.ListRequest(service, new string[] { YouTubeConstants.RequestSnippetPart });
        if (string.IsNullOrEmpty(channelId)) return channelVideos;
        searchRequest.ChannelId = channelId;
        searchRequest.MaxResults = 50;
        searchRequest.Order = SearchResource.ListRequest.OrderEnum.Date;
        bool requestNextPage = true;
        while (requestNextPage)
        {
            Func<SearchListResponse> getSearchResponse = (() =>
            {
                return searchRequest.Execute();
            });
            SearchListResponse? searchResponse = YouTubeRetryHelper.RetryCommand(intelligence, 100, getSearchResponse, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(10), client.LogMessage);
            if (searchResponse == null) throw new MilestoneException($"Could not get YouTube video list for channel id {searchRequest.ChannelId}");
            int count = 0;
            foreach (SearchResult searchResult in searchResponse.Items)
            {
                count++;
                if (count > 3) break;
                if (!YouTubeQuotaHelper.HasRemainingCalls(intelligence)) throw new MilestoneException("YouTube API rate limit exceeded");
                if (searchResult.Id.Kind != YouTubeConstants.VideoKind) continue;
                VideosResource.ListRequest videoRequest = new VideosResource.ListRequest(service, new string[] { YouTubeConstants.RequestContentDetailsPart, YouTubeConstants.RequestSnippetPart });
                videoRequest.Id = searchResult.Id.VideoId;
                videoRequest.MaxResults = 1;
                Func<VideoListResponse> getVideoResponse = (() =>
                {
                    return videoRequest.Execute();
                });
                VideoListResponse? videoResponse = YouTubeRetryHelper.RetryCommand(intelligence, 1, getVideoResponse, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(10), client.LogMessage);
                if (videoResponse == null) throw new MilestoneException($"Could not get YouTube video {videoRequest.Id}");
                Video videoDetails = videoResponse.Items.First();
                if (excludeVideos != null && excludeVideos.Find(resourceId => resourceId.VideoId == videoDetails.Id) != null)
                {
                    client.LogVideoSkipped();
                    continue;
                }
                channelVideos.Add(videoDetails);
            }
            requestNextPage = !string.IsNullOrEmpty(searchResponse.NextPageToken);
            searchRequest.PageToken = searchResponse.NextPageToken;
        }
        return channelVideos;
    }
}
