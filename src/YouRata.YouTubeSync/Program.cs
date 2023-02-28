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
using YouRata.Common.Configurations;
using YouRata.Common.GitHub;
using YouRata.Common.Proto;
using YouRata.Common.YouTube;
using YouRata.YouTubeSync.ConflictMonitor;
using YouRata.YouTubeSync.ErrataBulletin;
using YouRata.YouTubeSync.YouTube;
using static YouRata.Common.Proto.MilestoneActionIntelligence.Types;

using (YouTubeSyncCommunicationClient client = new YouTubeSyncCommunicationClient())
{
    System.Threading.Thread.Sleep(2000);
    YouTubeSyncActionIntelligence? milestoneInt = client.GetMilestoneActionIntelligence();
    if (milestoneInt == null) return;
    if (milestoneInt.Condition == MilestoneCondition.MilestoneBlocked) return;
    if (client.GetYouRataConfiguration().ActionCutOuts.DisableYouTubeSyncMilestone) return;
    client.Activate();

    ActionIntelligence actionInt = client.GetActionIntelligence();
    YouRataConfiguration config = client.GetYouRataConfiguration();
    GitHubActionEnvironment actionEnvironment = actionInt.GitHubActionEnvironment;
    TokenResponse? savedTokenResponse = JsonConvert.DeserializeObject<TokenResponse>(actionInt.TokenResponse);
    if (savedTokenResponse == null) return; /// throw an error
    GoogleAuthorizationCodeFlow authFlow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
    {
        ClientSecrets = new ClientSecrets
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
                    HttpClientInitializer = userCred,
                    BaseUri = "https://localhost"
                }))
    {
        List<ResourceId> ignoreResources = YouTubePlaylistHelper.GetPlaylistVideos(config.YouTube.ExcludePlaylists, ytService, client.LogMessage);
        GitHubClient ghClient = new GitHubClient(GitHubConstants.ProductHeader)
        {
            Credentials = new Credentials(actionEnvironment.ApiToken, AuthenticationType.Bearer)
        };
        ghClient.SetRequestTimeout(GitHubConstants.RequestTimeout);
        string[] repository = actionEnvironment.EnvGitHubRepository.Split("/");
        List<Video> videoList = YouTubeVideoHelper.GetChannelVideos(config.YouTube.ChannelId, ignoreResources, milestoneInt, ytService, client.LogMessage);
        foreach (Video video in videoList)
        {
            if (video.ContentDetails == null) continue;
            if (video.Snippet == null) continue;
            string errataBulletinPath = $"{ErrataBulletinConstants.ErrataRootDirectory}" +
                $"{video.Id}.md";
            if (!Path.Exists(Path.Combine(Directory.GetCurrentDirectory(), GitHubConstants.ErrataCheckoutPath, errataBulletinPath)))
            {
                string ytVideoTitle = WebUtility.HtmlDecode(video.Snippet.Title);
                string videoTitle = string.IsNullOrEmpty(ytVideoTitle) ? "Unknown Video" : ytVideoTitle;
                TimeSpan contentDuration = XmlConvert.ToTimeSpan(video.ContentDetails.Duration);
                ErrataBulletinBuilder bulletinBuilder = new ErrataBulletinBuilder(config.ErrataBulletin, videoTitle, contentDuration);
                string errataBulletin = bulletinBuilder.Build();

                CreateFileRequest createFile = new CreateFileRequest(videoTitle, errataBulletin, GitHubConstants.ErrataBranch);
                ghClient.Repository.Content.CreateFile(repository[0], repository[1], errataBulletinPath, createFile).Wait();

                if (!config.ActionCutOuts.DisableYouTubeVideoUpdate)
                {
                    string erattaLink =
                        string.Format(CultureInfo.InvariantCulture, "{0}/{1}/{2}",
                        actionEnvironment.EnvGitHubServerUrl,
                        actionEnvironment.EnvGitHubRepository,
                        errataBulletinPath);
                    string newDescription = YouTubeDescriptionErattaPublisher.GetAmendedDescription(video.Snippet.Description, erattaLink, config.YouTube);
                    YouTubeVideoHelper.UpdateVideoDescription(video, newDescription, ytService, client.LogMessage);
                }
                milestoneInt.VideosProcessed++;
            }
        }
    }
    client.SetMilestoneActionIntelligence(milestoneInt);
    Console.WriteLine(client.GetMilestoneActionIntelligence());
}
