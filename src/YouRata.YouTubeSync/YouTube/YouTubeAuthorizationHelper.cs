using System;
using System.Threading;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.YouTube.v3.Data;
using YouRata.Common.Milestone;
using YouRata.Common.YouTube;
using YouRata.YouTubeSync.ConflictMonitor;

namespace YouRata.YouTubeSync.YouTube;

internal static class YouTubeAuthorizationHelper
{

    public static TokenResponse RefreshToken(GoogleAuthorizationCodeFlow authFlow, string refreshToken, YouTubeSyncCommunicationClient client)
    {
        Func<TokenResponse> getTokenResponse = (() =>
        {
            Console.WriteLine("Try");
            return authFlow.RefreshTokenAsync(null, refreshToken, CancellationToken.None).Result;
        });
        TokenResponse? tokenResponse = YouTubeRetryHelper.RetryCommand(null, 0, getTokenResponse, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(10), client.LogMessage);
        if (tokenResponse == null) throw new MilestoneException("Could not refresh authorization token");
        return tokenResponse;
    }
}
