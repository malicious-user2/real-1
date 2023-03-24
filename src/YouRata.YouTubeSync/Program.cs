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
using Newtonsoft.Json.Linq;
using Octokit;
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
    YouTubeSyncWorkflow workflow = new YouTubeSyncWorkflow();
    if (!YouTubeAPIHelper.GetTokenResponse(workflow.StoredTokenResponse, out TokenResponse savedTokenResponse)) return;
    try
    {
        ActionReportLayout previousActionReport = client.GetPreviousActionReport();
        YouTubeQuotaHelper.SetPreviousActionReport(config.YouTube, client, milestoneInt, previousActionReport);
        GoogleAuthorizationCodeFlow authFlow = YouTubeAuthorizationHelper.GetFlow(workflow);
        if (savedTokenResponse.IsExpired(authFlow.Clock))
        {
            savedTokenResponse = YouTubeAuthorizationHelper.RefreshToken(authFlow, savedTokenResponse.RefreshToken, client);
            client.Keepalive();
            YouTubeAPIHelper.SaveTokenResponse(savedTokenResponse, actionInt.GitHubActionEnvironment, client.LogMessage);
            client.Keepalive();
        }
        UserCredential userCred = new UserCredential(authFlow, null, savedTokenResponse);
        List<string> processedVideos = new List<string>();
        using (YouTubeService ytService = YouTubeServiceHelper.GetService(workflow, userCred))
        {
            List<ResourceId> ignoreVideos = YouTubePlaylistHelper.GetPlaylistVideos(config.YouTube, milestoneInt, ytService, client);
            List<Video> videoList = YouTubeVideoHelper.GetRecentChannelVideos(config.YouTube, ignoreVideos, milestoneInt, ytService, client);
            List<Video> oustandingVideoList = new List<Video>();
            if (milestoneInt.HasOutstandingVideos)
            {
                oustandingVideoList = YouTubeVideoHelper.GetOutstandingChannelVideos(config.YouTube, ignoreVideos, milestoneInt, ytService, client);
                videoList.AddRange(oustandingVideoList.Where(outVideo => videoList.FirstOrDefault(recentVideo => recentVideo.Id == outVideo.Id) == null).ToList());
            }
            foreach (Video video in videoList)
            {
                if (video.ContentDetails == null) continue;
                if (video.Snippet == null) continue;
                if (processedVideos.Contains(video.Id)) continue;
                string errataBulletinPath = $"{ErrataBulletinConstants.ErrataRootDirectory}" +
                    $"{video.Id}.md";
                if (!Path.Exists(Path.Combine(workflow.Workspace, errataBulletinPath)))
                {
                    ErrataBulletinBuilder errataBulletinBuilder = ErrataBulletin.CreateBuilder(video, config.ErrataBulletin);
                    if (GitHubAPIClient.CreateContentFile(actionEnvironment,
                        errataBulletinBuilder.SnippetTitle,
                        errataBulletinBuilder.Build(),
                        errataBulletinPath,
                        client.LogMessage))
                    {
                        client.Keepalive();
                        if (!config.ActionCutOuts.DisableYouTubeVideoUpdate)
                        {
                            string erattaLink = YouTubeDescriptionErattaPublisher.GetErrataLink(actionEnvironment, errataBulletinPath);
                            string newDescription = YouTubeDescriptionErattaPublisher.GetAmendedDescription(video.Snippet.Description, erattaLink, config.YouTube);
                            YouTubeVideoHelper.UpdateVideoDescription(video, newDescription, milestoneInt, ytService, client);
                            client.Keepalive();
                        }
                    }
                    processedVideos.Add(video.Id);
                    milestoneInt.VideosProcessed++;
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



    Console.WriteLine(actionEnvironment.RateLimitCoreRemaining);
}
