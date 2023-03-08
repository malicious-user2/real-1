using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using Google.Protobuf;
using Google.Protobuf.Collections;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Octokit;
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
        client.Activate();
        YouTubeQuotaHelper.SetPreviousActionReport(config.YouTube, milestoneInt, previousActionReport);
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
                        HttpClientInitializer = userCred
                    }))
        {
            long lastPublishTime;
            List<ResourceId> ignoreResources = YouTubePlaylistHelper.GetPlaylistVideos(config.YouTube.ExcludePlaylists, milestoneInt, ytService, client);
            List<Video> videoList = YouTubeVideoHelper.GetChannelVideos(config.YouTube.ChannelId, out lastPublishTime, ignoreResources, milestoneInt, ytService, client);
            foreach (Video video in videoList)
            {
                if (video.ContentDetails == null) continue;
                if (video.Snippet == null) continue;
                string errataBulletinPath = $"{ErrataBulletinConstants.ErrataRootDirectory}" +
                    $"{video.Id}.md";
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
                    client.LogVideoProcessed(lastPublishTime);
                }
            }
        }
        client.LogAPIQueries(milestoneInt);
    }
    catch (Exception ex)
    {
        client.SetStatus(MilestoneCondition.MilestoneFailed);
        throw new MilestoneException("YouTubeSync failed", ex);
    }
    client.SetStatus(MilestoneCondition.MilestoneCompleted);
}
