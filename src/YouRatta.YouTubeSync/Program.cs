using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Xml;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Octokit;
using YouRatta.Common.Configurations;
using YouRatta.Common.GitHub;
using YouRatta.Common.Proto;
using YouRatta.Common.YouTube;
using YouRatta.YouTubeSync.ConflictMonitor;
using YouRatta.YouTubeSync.ErrataBulletin;
using YouRatta.YouTubeSync.YouTube;
using static YouRatta.Common.Proto.MilestoneActionIntelligence.Types;

using (YouTubeSyncCommunicationClient client = new YouTubeSyncCommunicationClient())
{
    System.Threading.Thread.Sleep(2000);
    if (client.GetMilestoneActionIntelligence().Condition == MilestoneCondition.MilestoneBlocked) return;
    if (client.GetYouRattaConfiguration().ActionCutOuts.DisableYouTubeSyncMilestone) return;
    client.Activate();

    ActionIntelligence actionInt = client.GetActionIntelligence();
    YouRattaConfiguration config = client.GetYouRattaConfiguration();
    GitHubActionEnvironment actionEnvironment = actionInt.GitHubActionEnvironment;
    TokenResponse? savedTokenResponse = JsonConvert.DeserializeObject<TokenResponse>(actionInt.TokenResponse);
    if (savedTokenResponse == null) return; /// throw an error
    GoogleAuthorizationCodeFlow authFlow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
    {
        ClientSecrets = new ClientSecrets()
        {
            ClientId = actionInt.AppClientId,
            ClientSecret = actionInt.AppClientSecret
        },
        Scopes = new[] { YouTubeService.Scope.YoutubeForceSsl }
    });
    if (savedTokenResponse.IsExpired(authFlow.Clock))
    {
        savedTokenResponse = authFlow.RefreshTokenAsync(null, savedTokenResponse.RefreshToken, CancellationToken.None).Result;
        YouTubeAPIHelper.SaveTokenResponse(savedTokenResponse, actionInt.GitHubActionEnvironment, client.LogMessage);
    }
    UserCredential userCred = new UserCredential(authFlow, null, savedTokenResponse);
    using (YouTubeService ytService
            = new YouTubeService(
                new BaseClientService.Initializer()
                {
                    ApiKey = actionInt.AppApiKey,
                    ApplicationName = YouTubeConstants.RequestApplicationName,
                    HttpClientInitializer = userCred
                }))
    {
        List<ResourceId> ignoreResources = YouTubePlaylistHelper.GetPlaylistVideos(config.YouTube.ExcludePlaylists, ytService);

        SearchResource.ListRequest searchRequest = new SearchResource.ListRequest(ytService, new string[] { YouTubeConstants.RequestSnippetPart });
        if (string.IsNullOrEmpty(config.YouTube.ChannelId)) return;
        searchRequest.ChannelId = config.YouTube.ChannelId;
        searchRequest.MaxResults = 50;
        bool requestNextPage = true;
        while (requestNextPage)
        {
            GitHubClient ghClient = new GitHubClient(GitHubConstants.ProductHeader)
            {
                Credentials = new Credentials(actionEnvironment.ApiToken, AuthenticationType.Bearer)
            };
            ghClient.SetRequestTimeout(GitHubConstants.RequestTimeout);
            string[] repository = actionEnvironment.EnvGitHubRepository.Split("/");
            SearchListResponse searchResponse = searchRequest.Execute();
            foreach (SearchResult searchResult in searchResponse.Items)
            {
                if (searchResult.Id.Kind != YouTubeConstants.VideoKind) continue;
                string errataBulletinPath = $"{ErrataBulletinConstants.ErrataRootDirectory}" +
                    $"{searchResult.Id.VideoId}.md";
                if (!Path.Exists(Path.Combine(Directory.GetCurrentDirectory(), GitHubConstants.ErrataBranch, errataBulletinPath)))
                {
                    Console.WriteLine(Path.Combine(Directory.GetCurrentDirectory(), GitHubConstants.ErrataBranch, errataBulletinPath));
                    VideosResource.ListRequest videoRequest = new VideosResource.ListRequest(ytService, new string[] { YouTubeConstants.RequestContentDetailsPart });
                    videoRequest.Id = searchResult.Id.VideoId;
                    videoRequest.MaxResults = 1;
                    VideoListResponse videoResponse = videoRequest.Execute();
                    Video videoDetails = videoResponse.Items.First();
                    ErrataBulletinBuilder bulletinBuilder = new ErrataBulletinBuilder(config.ErrataBulletin);
                    string ytVideoTitle = WebUtility.HtmlDecode(searchResult.Snippet.Title);
                    string videoTitle = string.IsNullOrEmpty(ytVideoTitle) ? "Unknown Video" : ytVideoTitle;
                    bulletinBuilder.SnippetTitle = videoTitle;
                    bulletinBuilder.ContentDuration = XmlConvert.ToTimeSpan(videoDetails.ContentDetails.Duration);
                    string errataBulletin = bulletinBuilder.Build();


                    CreateFileRequest createFile = new CreateFileRequest(videoTitle, errataBulletin, GitHubConstants.ErrataBranch);
                    ghClient.Repository.Content.CreateFile(repository[0], repository[1], errataBulletinPath, createFile).Wait();
                }

            }
            requestNextPage = !string.IsNullOrEmpty(searchResponse.NextPageToken);
            searchRequest.PageToken = searchResponse.NextPageToken;
        }


        foreach (string dir in Directory.GetFiles(Path.Combine(Directory.GetCurrentDirectory(), "errata")))
        {
            Console.WriteLine(dir);
        }

        Console.WriteLine(XmlConvert.ToTimeSpan("PT4M13S").ToString());
    }
}
