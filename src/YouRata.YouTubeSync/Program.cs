using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Http;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Octokit;
using RichardSzalay.MockHttp;
using YouRata.Common.ActionReport;
using YouRata.Common.Configurations;
using YouRata.Common.GitHub;
using YouRata.Common.Milestone;
using YouRata.Common.Proto;
using YouRata.Common.YouTube;
using YouRata.YouTubeSync.ConflictMonitor;
using YouRata.YouTubeSync.ErrataBulletin;
using YouRata.YouTubeSync.YouTube;
using static YouRata.Common.Proto.MilestoneActionIntelligence.Types;

using (YouTubeSyncCommunicationClient client = new YouTubeSyncCommunicationClient())
{
    YouTubeSyncActionIntelligence? milestoneInt = client.GetMilestoneActionIntelligence();
    if (client.GetYouRataConfiguration().ActionCutOuts.DisableYouTubeSyncMilestone) return;
    if (milestoneInt == null) return;
    if (milestoneInt.Condition == MilestoneCondition.MilestoneBlocked) return;
    ActionIntelligence actionInt;
    GitHubActionEnvironment actionEnvironment;
    YouRataConfiguration config;
    MilestoneVariablesHelper.CreateRuntimeVariables(client, out actionInt, out config, out actionEnvironment);
    if (!YouTubeAPIHelper.IsValidTokenResponse(actionInt.TokenResponse)) return;
    TokenResponse? savedTokenResponse = JsonConvert.DeserializeObject<TokenResponse>(actionInt.TokenResponse);
    if (savedTokenResponse == null) return;
    string? workspace = Environment.GetEnvironmentVariable(GitHubConstants.GitHubWorkspaceVariable);
    if (workspace == null) return;
    ActionReportLayout previousActionReport = client.GetPreviousActionReport();
    try
    {
        client.Activate(ref milestoneInt);
        YouTubeQuotaHelper.SetPreviousActionReport(config.YouTube, client, milestoneInt, previousActionReport);
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



        var mockHttp = new MockHttpMessageHandler();
        mockHttp.When(HttpMethod.Put, "https://youtube.googleapis.com/youtube/v3/videos*").Respond(HttpStatusCode.OK);
        mockHttp.Fallback.Respond(new HttpClient());

        using (YouTubeService ytService
                = new YouTubeService(
                    new BaseClientService.Initializer()
                    {
                        ApiKey = actionInt.AppApiKey,
                        ApplicationName = YouTubeConstants.RequestApplicationName,
                        HttpClientInitializer = userCred,
                        HttpClientFactory = new MockHttpClientFactory(mockHttp)
                    }))
        {
            ytService.HttpClient.Timeout = YouTubeConstants.RequestTimeout;
            List<ResourceId> ignoreResources = YouTubePlaylistHelper.GetPlaylistVideos(config.YouTube, milestoneInt, ytService, client);
            List<Video> videoList = YouTubeVideoHelper.GetRecentChannelVideos(config.YouTube, ignoreResources, milestoneInt, ytService, client);
            List<Video> oustandingVideoList = new List<Video>();
            if (milestoneInt.HasOutstandingVideos)
            {
                oustandingVideoList = YouTubeVideoHelper.GetOutstandingChannelVideos(config.YouTube, ignoreResources, milestoneInt, ytService, client);
                videoList.AddRange(oustandingVideoList);
            }
            foreach (Video video in videoList)
            {
                if (video.ContentDetails == null) continue;
                if (video.Snippet == null) continue;
                string errataBulletinPath = $"{ErrataBulletinConstants.ErrataRootDirectory}" +
                    $"{video.Id}.md";
                Console.WriteLine(Path.Combine(workspace, GitHubConstants.ErrataCheckoutPath, errataBulletinPath));
                Console.WriteLine(Path.Exists(Path.Combine(workspace, GitHubConstants.ErrataCheckoutPath, errataBulletinPath)));
                if (!Path.Exists(Path.Combine(workspace, GitHubConstants.ErrataCheckoutPath, errataBulletinPath)))
                {
                    string ytVideoTitle = WebUtility.HtmlDecode(video.Snippet.Title);
                    string videoTitle = string.IsNullOrEmpty(ytVideoTitle) ? "Unknown Video" : ytVideoTitle;
                    TimeSpan contentDuration = XmlConvert.ToTimeSpan(video.ContentDetails.Duration);
                    ErrataBulletinBuilder bulletinBuilder = new ErrataBulletinBuilder(config.ErrataBulletin, videoTitle, contentDuration);
                    string errataBulletin = bulletinBuilder.Build();

                    if (GitHubAPIClient.CreateContentFile(actionEnvironment, videoTitle, errataBulletin, errataBulletinPath, client.LogMessage))
                    {
                        if (!config.ActionCutOuts.DisableYouTubeVideoUpdate)
                        {
                            string erattaLink =
                                string.Format(CultureInfo.InvariantCulture, "{0}/{1}/{2}",
                                actionEnvironment.EnvGitHubServerUrl,
                                actionEnvironment.EnvGitHubRepository,
                                errataBulletinPath);
                            string newDescription = YouTubeDescriptionErattaPublisher.GetAmendedDescription(video.Snippet.Description, erattaLink, config.YouTube);
                            YouTubeVideoHelper.UpdateVideoDescription(video, newDescription, milestoneInt, ytService, client);
                        }
                    }
                    client.LogVideoProcessed();
                }
            }
            milestoneInt.HasOutstandingVideos = (oustandingVideoList.Count > 0 || milestoneInt.LastQueryTime == 0);
            milestoneInt.LastQueryTime = DateTimeOffset.Now.ToUnixTimeSeconds();
            client.SetMilestoneActionIntelligence(milestoneInt);
        }
    }
    catch (Exception ex)
    {
        client.LogAPIQueries(milestoneInt);
        client.SetStatus(MilestoneCondition.MilestoneFailed);
        throw new MilestoneException("YouTubeSync failed", ex);
    }
    client.SetStatus(MilestoneCondition.MilestoneCompleted);
}

public class MockHttpClientFactory : HttpClientFactory
{
    private HttpMessageHandler Handler { get; set; }

    public MockHttpClientFactory(HttpMessageHandler handler)
    {
        Handler = handler;
    }

    protected override HttpMessageHandler CreateHandler(CreateHttpClientArgs args)
    {
        return Handler;
    }
}
