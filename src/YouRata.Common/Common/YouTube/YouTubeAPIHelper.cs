using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Requests;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Json;
using Google.Apis.YouTube.v3;
using Newtonsoft.Json;
using Octokit;
using YouRata.Common.GitHub;
using YouRata.Common.Proto;

namespace YouRata.Common.YouTube;

public static class YouTubeAPIHelper
{
    public static bool IsValidTokenResponse(string response)
    {
        if (string.IsNullOrEmpty(response)) return false;
        JsonSerializerSettings serializerSettings = new JsonSerializerSettings()
        {
            MissingMemberHandling = MissingMemberHandling.Error
        };
        try
        {
            TokenResponse? tokenResponse = JsonConvert.DeserializeObject<TokenResponse>(response, serializerSettings);
            if (tokenResponse == null) return false;
            if (string.IsNullOrEmpty(tokenResponse.AccessToken)) return false;
            if (string.IsNullOrEmpty(tokenResponse.RefreshToken)) return false;
        }
        catch
        {
            return false;
        }
        return true;
    }

    public static GoogleAuthorizationCodeFlow GetFlow(string clientId, string clientSecret)
    {
        GoogleAuthorizationCodeFlow flow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
        {
            ClientSecrets = new ClientSecrets
            {
                ClientId = clientId,
                ClientSecret = clientSecret
            },
            Scopes = new[] { YouTubeService.Scope.YoutubeForceSsl }
        });
        return flow;
    }

    public static AuthorizationCodeTokenRequest GetTokenRequest(string code, string clientId, string clientSecret)
    {
        AuthorizationCodeTokenRequest authorizationCodeTokenRequest = new AuthorizationCodeTokenRequest
        {
            Scope = YouTubeService.Scope.YoutubeForceSsl,
            RedirectUri = GoogleAuthConsts.LocalhostRedirectUri,
            Code = code,
            ClientId = clientId,
            ClientSecret = clientSecret
        };
        return authorizationCodeTokenRequest;
    }

    public static void SaveTokenResponse(TokenResponse response, GitHubActionEnvironment actionEnvironment, Action<string> logger)
    {
        string tokenResponseString = JsonConvert.SerializeObject(response, Formatting.None);
        GitHubAPIClient.CreateOrUpdateSecret(actionEnvironment, YouRataConstants.StoredTokenResponseVariable, tokenResponseString, logger);
        GitHubAPIClient.DeleteSecret(actionEnvironment, YouTubeConstants.RedirectCodeVariable, (_) => { });
    }

    public static TokenResponse? ExchangeAuthorizationCode(AuthorizationCodeTokenRequest request, GoogleAuthorizationCodeFlow flow)
    {
        TokenResponse? authorizationCodeTokenResponse;
        using (HttpClient tokenHttpClient = new HttpClient())
        {
            authorizationCodeTokenResponse = request.ExecuteAsync(tokenHttpClient, flow.TokenServerUrl, CancellationToken.None, flow.Clock).Result;
        }
        return authorizationCodeTokenResponse;
    }
}
