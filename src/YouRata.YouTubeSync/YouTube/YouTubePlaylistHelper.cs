using System;
using System.Collections.Generic;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using YouRata.Common.Configurations;
using YouRata.Common.Milestone;
using YouRata.Common.YouTube;
using YouRata.YouTubeSync.ConflictMonitor;
using static YouRata.Common.Proto.MilestoneActionIntelligence.Types;

namespace YouRata.YouTubeSync.YouTube;

internal static class YouTubePlaylistHelper
{
    public static List<ResourceId> GetPlaylistVideos(YouTubeConfiguration config, YouTubeSyncActionIntelligence intelligence, YouTubeService service, YouTubeSyncCommunicationClient client)
    {
        if (!YouTubeQuotaHelper.HasRemainingCalls(intelligence)) throw new MilestoneException("YouTube API rate limit exceeded");
        List<string> videoIds = new List<string>();
        List<ResourceId> playlistResourceIds = new List<ResourceId>();
        string[] playlists = config.ExcludePlaylists;
        if (playlists == null) return playlistResourceIds;
        foreach (string playlist in playlists)
        {
            if (playlist == null) continue;
            PlaylistItemsResource.ListRequest playlistRequest = new PlaylistItemsResource.ListRequest(service, new string[] { YouTubeConstants.RequestSnippetPart, YouTubeConstants.RequestStatusPart });
            playlistRequest.PlaylistId = playlist;
            playlistRequest.MaxResults = 50;
            bool requestNextPage = true;
            while (requestNextPage)
            {
                Func<PlaylistItemListResponse> getPlaylistResponse = (() =>
                {
                    return playlistRequest.Execute();
                });
                PlaylistItemListResponse? playlistResponse = YouTubeRetryHelper.RetryCommand(intelligence, YouTubeConstants.PlaylistItemListQuotaCost, getPlaylistResponse, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(10), client.LogMessage);
                if (playlistResponse == null) throw new MilestoneException("Could not get YouTube playlist items");
                foreach (PlaylistItem playlistItem in playlistResponse.Items)
                {
                    if (playlistItem.Snippet.ResourceId.Kind != YouTubeConstants.VideoKind) continue;
                    if (videoIds.Contains(playlistItem.Snippet.ResourceId.VideoId)) continue;
                    if (playlistItem.Status.PrivacyStatus == YouTubeConstants.PrivacyStatusPrivate) continue;
                    playlistResourceIds.Add(playlistItem.Snippet.ResourceId);
                    videoIds.Add(playlistItem.Snippet.ResourceId.VideoId);
                }
                requestNextPage = !string.IsNullOrEmpty(playlistResponse.NextPageToken);
                playlistRequest.PageToken = playlistResponse.NextPageToken;
            }
        }
        return playlistResourceIds;
    }
}
