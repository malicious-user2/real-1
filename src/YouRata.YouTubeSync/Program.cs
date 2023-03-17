using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Xml;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using YouRata.Common.ActionReport;
using YouRata.Common.Configurations;
using YouRata.Common.GitHub;
using YouRata.Common.Milestone;
using YouRata.Common.Proto;
using YouRata.Common.YouTube;
using YouRata.YouTubeSync.ConflictMonitor;
using YouRata.YouTubeSync.ErrataBulletin;
using YouRata.YouTubeSync.Workflow;
using YouRata.YouTubeSync.YouTube;
using static YouRata.Common.Proto.MilestoneActionIntelligence.Types;

using (YouTubeSyncCommunicationClient client = new YouTubeSyncCommunicationClient())
{
    if (!client.Activate(out YouTubeSyncActionIntelligence milestoneInt)) return;
    if (client.GetYouRataConfiguration().ActionCutOuts.DisableYouTubeSyncMilestone) return;
    MilestoneVariablesHelper.CreateRuntimeVariables(client, out ActionIntelligence actionInt, out YouRataConfiguration config, out GitHubActionEnvironment actionEnvironment);
    if (!YouTubeAPIHelper.GetTokenResponse(actionInt.TokenResponse, out TokenResponse savedTokenResponse)) return;
    YouTubeSyncWorkflow workflow = new YouTubeSyncWorkflow();


    ActionReportLayout previousActionReport = client.GetPreviousActionReport();
    try
    {
        YouTubeQuotaHelper.SetPreviousActionReport(config.YouTube, client, milestoneInt, previousActionReport);
        GoogleAuthorizationCodeFlow authFlow = YouTubeAuthorizationHelper.GetFlow(actionInt);
        if (savedTokenResponse.IsExpired(authFlow.Clock))
        {
            savedTokenResponse = YouTubeAuthorizationHelper.RefreshToken(authFlow, savedTokenResponse.RefreshToken, client);
            YouTubeAPIHelper.SaveTokenResponse(savedTokenResponse, actionInt.GitHubActionEnvironment, client.LogMessage);
        }
        UserCredential userCred = new UserCredential(authFlow, null, savedTokenResponse);
        List<string> completedVideos = new List<string>();
        using (YouTubeService ytService = YouTubeServiceHelper.GetService(actionInt, userCred))
        {
            List<ResourceId> ignoreResources = YouTubePlaylistHelper.GetPlaylistVideos(config.YouTube, milestoneInt, ytService, client);
            List<Video> videoList = YouTubeVideoHelper.GetRecentChannelVideos(config.YouTube, ignoreResources, milestoneInt, ytService, client);
            List<Video> oustandingVideoList = new List<Video>();
            if (milestoneInt.HasOutstandingVideos)
            {
                oustandingVideoList = YouTubeVideoHelper.GetOutstandingChannelVideos(config.YouTube, ignoreResources, milestoneInt, ytService, client);
                videoList.AddRange(oustandingVideoList.Where(outVideo => videoList.FirstOrDefault(recentVideo => recentVideo.Id == outVideo.Id) == null).ToList());
            }
            foreach (Video video in videoList)
            {
                if (video.ContentDetails == null) continue;
                if (video.Snippet == null) continue;
                if (completedVideos.Contains(video.Id)) continue;
                string errataBulletinPath = $"{ErrataBulletinConstants.ErrataRootDirectory}" +
                    $"{video.Id}.md";
                Console.WriteLine(Path.Combine(workflow.Workspace, GitHubConstants.ErrataCheckoutPath, errataBulletinPath));
                Console.WriteLine(Path.Exists(Path.Combine(workflow.Workspace, GitHubConstants.ErrataCheckoutPath, errataBulletinPath)));
                if (!Path.Exists(Path.Combine(workflow.Workspace, GitHubConstants.ErrataCheckoutPath, errataBulletinPath)))
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
                    completedVideos.Add(video.Id);
                    milestoneInt.VideosProcessed++;
                }
            }
            Console.WriteLine(oustandingVideoList.Count);
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
