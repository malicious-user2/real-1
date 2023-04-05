// Copyright (c) 2023 battleship-systems.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using YouRata.Common;
using YouRata.Common.ActionReport;
using YouRata.Common.Configuration;
using YouRata.Common.GitHub;
using YouRata.Common.Milestone;
using YouRata.Common.Proto;
using YouRata.Common.YouTube;
using YouRata.YouTubeSync.ConflictMonitor;
using YouRata.YouTubeSync.ErrataBulletin;
using YouRata.YouTubeSync.Workflow;
using YouRata.YouTubeSync.YouTube;
using static YouRata.Common.Proto.MilestoneActionIntelligence.Types;

/// ---------------------------------------------------------------------------------------------
/// The YouTubeSync milestone makes a video errata bulletin for each video in a YouTube channel.
/// Each YouTube video description is updated to add a link to the errata bulletin on GitHub.
/// Control is started from the Run YouRata action in the event the TOKEN_RESPONSE environment
/// variable contains a valid TokenResponse. Channels with extensive video history will require
/// multiple days to create all errata bulletins.
/// ---------------------------------------------------------------------------------------------

using (YouTubeSyncCommunicationClient client = new YouTubeSyncCommunicationClient())
{
    if (!client.Activate(out YouTubeSyncActionIntelligence milestoneInt)) return;
    if (client.GetYouRataConfiguration().ActionCutOuts.DisableYouTubeSyncMilestone) return;
    MilestoneVariablesHelper.CreateRuntimeVariables(client, out ActionIntelligence actionInt, out YouRataConfiguration config,
        out GitHubActionEnvironment actionEnvironment);
    YouTubeSyncWorkflow workflow = new YouTubeSyncWorkflow();
    if (!YouTubeAuthHelper.GetTokenResponse(workflow.StoredTokenResponse, out TokenResponse savedTokenResponse)) return;
    try
    {
        ActionReportLayout previousActionReport = client.GetPreviousActionReport();
        YouTubeQuotaHelper.SetPreviousActionReport(config.YouTube, client, milestoneInt, previousActionReport);
        GoogleAuthorizationCodeFlow authFlow = YouTubeAuthHelper.GetFlow(workflow.ProjectClientId, workflow.ProjectClientSecret);
        if (savedTokenResponse.IsExpired(authFlow.Clock))
        {
            savedTokenResponse = YouTubeAuthHelper.RefreshToken(authFlow, savedTokenResponse.RefreshToken, client.LogMessage);
            client.Keepalive();
            YouTubeAuthHelper.SaveTokenResponse(savedTokenResponse, actionInt.GitHubActionEnvironment, client.LogMessage);
            client.Keepalive();
        }

        UserCredential userCred = new UserCredential(authFlow, null, savedTokenResponse);
        List<string> processedVideos = new List<string>();
        using (YouTubeService ytService = YouTubeServiceHelper.GetService(workflow, userCred))
        {
            List<ResourceId> ignoreVideos = YouTubePlaylistHelper.GetPlaylistVideos(config.YouTube, milestoneInt, ytService, client);
            List<Video> videoList =
                YouTubeVideoHelper.GetRecentChannelVideos(config.YouTube, ignoreVideos, milestoneInt, ytService, client);
            List<Video> oustandingVideoList = new List<Video>();
            if (milestoneInt.HasOutstandingVideos)
            {
                oustandingVideoList =
                    YouTubeVideoHelper.GetOutstandingChannelVideos(config.YouTube, ignoreVideos, milestoneInt, ytService, client);
                videoList.AddRange(oustandingVideoList
                    .Where(outVideo => videoList.FirstOrDefault(recentVideo => recentVideo.Id == outVideo.Id) == null).ToList());
            }

            foreach (Video video in videoList)
            {
                if (video.ContentDetails == null) continue;
                if (video.Snippet == null) continue;
                if (processedVideos.Contains(video.Id)) continue;
                string errataBulletinPath = $"{YouRataConstants.ErrataRootDirectory}" +
                                            $"{video.Id}.md";
                if (!Path.Exists(Path.Combine(workflow.Workspace, errataBulletinPath)))
                {
                    ErrataBulletinBuilder errataBulletinBuilder = ErrataBulletinFactory.CreateBuilder(video, config.ErrataBulletin);
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
                            string newDescription =
                                YouTubeDescriptionErattaPublisher.GetAmendedDescription(video.Snippet.Description, erattaLink,
                                    config.YouTube);
                            if (newDescription.Length <= YouTubeConstants.MaxDescriptionLength)
                            {
                                YouTubeVideoHelper.UpdateVideoDescription(video, newDescription, milestoneInt, ytService, client);
                            }

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
}
