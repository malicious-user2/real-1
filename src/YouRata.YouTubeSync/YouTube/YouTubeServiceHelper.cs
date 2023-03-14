using System;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using YouRata.Common.Proto;
using YouRata.Common.YouTube;

namespace YouRata.YouTubeSync.YouTube;

internal static class YouTubeServiceHelper
{
    public static YouTubeService GetService(ActionIntelligence actionInt, UserCredential userCred)
    {
        YouTubeService ytService = new YouTubeService(
                    new BaseClientService.Initializer()
                    {
                        ApiKey = actionInt.AppApiKey,
                        ApplicationName = YouTubeConstants.RequestApplicationName,
                        HttpClientInitializer = userCred
                    });
        ytService.HttpClient.Timeout = YouTubeConstants.RequestTimeout;
        return ytService;
    }
}
