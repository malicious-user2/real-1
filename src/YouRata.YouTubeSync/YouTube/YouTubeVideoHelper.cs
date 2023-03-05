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
    public static void UpdateVideoDescription(Video video, string description, YouTubeService service, YouTubeSyncCommunicationClient client)
    {
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
        YouTubeRetryHelper.RetryCommand(videoUpdate, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(10), client.LogMessage);
    }

    public static List<Video> GetChannelVideos(string channelId, List<ResourceId> excludeVideos, YouTubeService service, YouTubeSyncCommunicationClient client)
    {
        List<Video> channelVideos = new List<Video>();
        string[] lines = File.ReadAllLines(Directory.GetCurrentDirectory() + "/prod.txt");

            foreach (string searchResult in lines)
            {
                VideosResource.ListRequest videoRequest = new VideosResource.ListRequest(service, new string[] { YouTubeConstants.RequestContentDetailsPart, YouTubeConstants.RequestSnippetPart });
                videoRequest.Id = searchResult.Replace("\n", "").Replace("\r", ""); ;
                videoRequest.MaxResults = 1;
            Console.WriteLine(videoRequest.Id);
                Func<VideoListResponse> getVideoResponse = (() =>
                {
                    return videoRequest.Execute();
                });
                VideoListResponse? videoResponse = YouTubeRetryHelper.RetryCommand(getVideoResponse, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(10), client.LogMessage);
                if (videoResponse == null) throw new MilestoneException($"Could not get YouTube video {videoRequest.Id}");
                Video videoDetails = videoResponse.Items.First();
                if (excludeVideos != null && excludeVideos.Find(resourceId => resourceId.VideoId == videoDetails.Id) != null)
                {
                    client.LogVideoSkipped();
                    continue;
                }
                channelVideos.Add(videoDetails);
            }
        return channelVideos;
    }
}
