using System;
using System.Net;
using System.Xml;
using Google.Apis.YouTube.v3.Data;
using YouRata.Common.Configurations;

namespace YouRata.YouTubeSync.ErrataBulletin;

internal static class ErrataBulletin
{

    public static ErrataBulletinBuilder CreateBuilder(Video video, ErrataBulletinConfiguration config)
    {
        string videoTitle = WebUtility.HtmlDecode(video.Snippet.Title) ?? "Unknown Video";
        TimeSpan contentDuration = XmlConvert.ToTimeSpan(video.ContentDetails.Duration);
        ErrataBulletinBuilder bulletinBuilder = new ErrataBulletinBuilder(config, videoTitle, contentDuration);
        return bulletinBuilder;
    }
}
