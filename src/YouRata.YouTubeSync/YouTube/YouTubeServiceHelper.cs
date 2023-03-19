using System;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using YouRata.Common.Proto;
using YouRata.Common.YouTube;
using YouRata.YouTubeSync.Workflow;

namespace YouRata.YouTubeSync.YouTube;

internal static class YouTubeServiceHelper
{
    public static YouTubeService GetService(YouTubeSyncWorkflow workflow, UserCredential userCred)
    {
        YouTubeService ytService = new YouTubeService(
                    new BaseClientService.Initializer()
                    {
                        ApiKey = workflow.ProjectApiKey,
                        ApplicationName = YouTubeConstants.RequestApplicationName,
                        HttpClientInitializer = userCred
                    });
        ytService.HttpClient.Timeout = YouTubeConstants.RequestTimeout;
        return ytService;
    }
}
