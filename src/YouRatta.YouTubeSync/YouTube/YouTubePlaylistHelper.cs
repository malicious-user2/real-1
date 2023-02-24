using System;
using System.Collections.Generic;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using YouRatta.Common.YouTube;

namespace YouRatta.YouTubeSync.YouTube;

internal static class YouTubePlaylistHelper
{
    public static List<ResourceId> GetPlaylistVideos(string[]? playlists, YouTubeService service)
    {
        List<string> videoIds = new List<string>();
        List<ResourceId> playlistResourceIds = new List<ResourceId>();
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
                PlaylistItemListResponse playlistResponse = playlistRequest.Execute();
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
