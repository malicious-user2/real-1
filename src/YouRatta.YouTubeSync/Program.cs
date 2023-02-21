using System;
using System.Threading;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using Newtonsoft.Json;
using Octokit;
using YouRatta.Common.GitHub;
using YouRatta.Common.Proto;
using YouRatta.Common.YouTube;
using YouRatta.YouTubeSync.ConflictMonitor;
using static YouRatta.Common.Proto.MilestoneActionIntelligence.Types;

using (YouTubeSyncCommunicationClient client = new YouTubeSyncCommunicationClient())
{
    System.Threading.Thread.Sleep(2000);
    if (client.GetMilestoneActionIntelligence().Condition == MilestoneCondition.MilestoneBlocked) return;
    if (client.GetYouRattaConfiguration().ActionCutOuts.DisableYouTubeSyncMilestone) return;

    client.Activate();

    ActionIntelligence actionInt = client.GetActionIntelligence();

    TokenResponse? res = JsonConvert.DeserializeObject<TokenResponse>(actionInt.TokenResponse);
    GoogleAuthorizationCodeFlow flow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
    {
        ClientSecrets = new Google.Apis.Auth.OAuth2.ClientSecrets()
        {
            ClientId = "573861238622-2qjkd0bq0n8d4ii3gpj1ipun3sk2s2ra.apps.googleusercontent.com",
            ClientSecret = "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"
        },
        Scopes = new[] { YouTubeService.Scope.YoutubeForceSsl }
    });
    UserCredential cred1 = new UserCredential(flow, "user", res);
    Console.WriteLine(cred1.UserId);
    if (res == null) return;
    if (res.IsExpired(flow.Clock))
    {
        res = flow.RefreshTokenAsync("user", res.RefreshToken, CancellationToken.None).Result;
    }
    YouTubeService yt = new YouTubeService(new Google.Apis.Services.BaseClientService.Initializer()
    {
        ApiKey = "AIzaSyAt13lyqRkogZxUBBtu_eXY0FWj7Fss4Ro",
        ApplicationName = GitHubConstants.ProductHeader.Name,
        HttpClientInitializer = cred1
    });

    PlaylistItemsResource.ListRequest req = new PlaylistItemsResource.ListRequest(yt, "snippet");
    req.PlaylistId = "PLALOZV63REeuCwSXu59LxGeDFv80GzVdX";
    req.MaxResults = 100;

    PlaylistItemListResponse res2 = req.ExecuteAsync().Result;

    foreach (PlaylistItem item in res2.Items)
    {
        Console.WriteLine(item.Snippet.ResourceId.VideoId);
    }



}

Console.ReadLine();
