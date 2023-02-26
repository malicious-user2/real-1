using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Xml;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using YouRatta.Common.YouTube;
using YouRatta.YouTubeSync.ErrataBulletin;

namespace YouRatta.YouTubeSync.YouTube;

internal static class YouTubeVideoHelper
{

    public static List<Video> GetChannelVideos(string channelId, List<ResourceId> excludeVideos, YouTubeService service)
    {
        List<Video> channelVideos = new List<Video>();
        SearchResource.ListRequest searchRequest = new SearchResource.ListRequest(service, new string[] { YouTubeConstants.RequestSnippetPart });
        if (string.IsNullOrEmpty(channelId)) return channelVideos;
        searchRequest.ChannelId = channelId;
        searchRequest.MaxResults = 50;
        bool requestNextPage = true;
        while (requestNextPage)
        {
            SearchListResponse searchResponse = searchRequest.Execute();
            foreach (SearchResult searchResult in searchResponse.Items)
            {
                if (searchResult.Id.Kind != YouTubeConstants.VideoKind) continue;
                VideosResource.ListRequest videoRequest = new VideosResource.ListRequest(service, new string[] { YouTubeConstants.RequestContentDetailsPart, YouTubeConstants.RequestSnippetPart });
                videoRequest.Id = searchResult.Id.VideoId;
                videoRequest.MaxResults = 1;
                VideoListResponse videoResponse = videoRequest.Execute();
                Video videoDetails = videoResponse.Items.First();
                if (excludeVideos != null && excludeVideos.Find(resourceId => resourceId.VideoId == videoDetails.Id) != null) continue;
                channelVideos.Add(videoDetails);
            }
            requestNextPage = !string.IsNullOrEmpty(searchResponse.NextPageToken);
            searchRequest.PageToken = searchResponse.NextPageToken;
        }
        return channelVideos;
    }
}
