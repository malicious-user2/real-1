using System;
using System.Collections.Generic;
using System.Globalization;
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
    YouTubeSyncActionIntelligence? milestoneInt = client.GetMilestoneActionIntelligence();
    if (milestoneInt == null) return;
    if (milestoneInt.Condition == MilestoneCondition.MilestoneBlocked) return;
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
        GitHubClient ghClient = new GitHubClient(GitHubConstants.ProductHeader)
        {
            Credentials = new Credentials(actionEnvironment.ApiToken, AuthenticationType.Bearer)
        };
        ghClient.SetRequestTimeout(GitHubConstants.RequestTimeout);
        string[] repository = actionEnvironment.EnvGitHubRepository.Split("/");
        List<Video> videoList = YouTubeVideoHelper.GetChannelVideos(config.YouTube.ChannelId, ignoreResources, ytService);
        foreach (Video video in videoList)
        {
            string errataBulletinPath = $"{ErrataBulletinConstants.ErrataRootDirectory}" +
                $"{video.Id}.md";
            if (!Path.Exists(Path.Combine(Directory.GetCurrentDirectory(), GitHubConstants.ErrataCheckoutPath, errataBulletinPath)))
            {
                ErrataBulletinBuilder bulletinBuilder = new ErrataBulletinBuilder(config.ErrataBulletin);
                string ytVideoTitle = WebUtility.HtmlDecode(video.Snippet.Title);
                string videoTitle = string.IsNullOrEmpty(ytVideoTitle) ? "Unknown Video" : ytVideoTitle;
                bulletinBuilder.SnippetTitle = videoTitle;
                bulletinBuilder.ContentDuration = XmlConvert.ToTimeSpan(video.ContentDetails.Duration);
                string errataBulletin = bulletinBuilder.Build();

                CreateFileRequest createFile = new CreateFileRequest(videoTitle, errataBulletin, GitHubConstants.ErrataBranch);
                ghClient.Repository.Content.CreateFile(repository[0], repository[1], errataBulletinPath, createFile).Wait();

                string erattaLink =
                    string.Format(CultureInfo.InvariantCulture, "{0}/{1}/{2}",
                    actionEnvironment.EnvGitHubServerUrl,
                    actionEnvironment.EnvGitHubRepository,
                    errataBulletinPath);
                string newDescription = YouTubeDescriptionErattaPublisher.GetAmendedDescription(video.Snippet.Description, erattaLink, config.YouTube);
                video.Snippet.Description = newDescription;
                video.ContentDetails = null;
                VideosResource.UpdateRequest videoUpdate = new VideosResource.UpdateRequest(ytService, video, new string[] { YouTubeConstants.RequestSnippetPart });

                videoUpdate.Execute();

            }
        }
    }
}
