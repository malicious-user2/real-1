using System;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Requests;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Json;
using Google.Apis.YouTube.v3;
using Google.Protobuf;
using Microsoft.VisualBasic;
using Newtonsoft.Json;
using YouRata.Common;
using YouRata.Common.Configurations;
using YouRata.Common.GitHub;
using YouRata.Common.Milestone;
using YouRata.Common.Proto;
using YouRata.Common.YouTube;
using YouRata.InitialSetup.ConflictMonitor;
using YouRata.InitialSetup.Workflow;
using static YouRata.Common.Proto.MilestoneActionIntelligence.Types;

using (InitialSetupCommunicationClient client = new InitialSetupCommunicationClient())
{
    InitialSetupActionIntelligence? milestoneInt = client.GetMilestoneActionIntelligence();
    if (client.GetYouRataConfiguration().ActionCutOuts.DisableInitialSetupMilestone) return;
    if (milestoneInt == null) return;
    if (milestoneInt.Condition == MilestoneCondition.MilestoneBlocked) return;
    InitialSetupWorkflow workflow = new InitialSetupWorkflow();
    ActionIntelligence actionInt;
    GitHubActionEnvironment actionEnvironment;
    YouRataConfiguration config;
    MilestoneVariablesHelper.CreateRuntimeVariables(client, out actionInt, out config, out actionEnvironment);
    try
    {
        milestoneInt = client.Activate();
        bool canContinue = true;
        if (string.IsNullOrEmpty(actionEnvironment.ApiToken))
        {
            Console.WriteLine("Entering actions secrets section");
            Console.WriteLine($"Create an action secret {GitHubConstants.ApiTokenVariable} to store GitHub personal access token");
            canContinue = false;
        }
        if (canContinue && (!Regex.IsMatch(actionEnvironment.ApiToken, @"^ghp_[a-zA-Z0-9]{36}$")))
        {
            Console.WriteLine("Entering actions secrets verify section");
            Console.WriteLine($"Action secret {GitHubConstants.ApiTokenVariable} is not a GitHub personal access token");
            canContinue = false;
        }
        if (canContinue && ((string.IsNullOrEmpty(actionInt.AppClientSecret) || string.IsNullOrEmpty(actionInt.AppClientId)) ||
                            (actionInt.AppClientSecret.Equals("empty") || actionInt.AppClientId.Equals("empty"))))
        {
            Console.WriteLine("Entering actions variables section");
            if (!config.ActionCutOuts.DisableUnsupportedGitHubAPI)
            {
                UnsupportedGitHubAPIClient.CreateVariable(actionEnvironment, YouTubeConstants.ProjectClientIdVariable, "empty", client.LogMessage);
                UnsupportedGitHubAPIClient.CreateVariable(actionEnvironment, YouTubeConstants.ProjectClientSecretsVariable, "empty", client.LogMessage);
                Console.WriteLine("Fill repository variables and run action again");
            }
            else
            {
                Console.WriteLine($"Create an action variable {YouTubeConstants.ProjectClientIdVariable} to store Google API client ID");
                Console.WriteLine($"Create an action variable {YouTubeConstants.ProjectClientSecretsVariable} to store Google API client secrets");
            }
            canContinue = false;
        }
        if (canContinue && (string.IsNullOrEmpty(actionInt.AppApiKey) || actionInt.AppApiKey.Equals("empty")))
        {
            Console.WriteLine("Entering Google API key section");
            GitHubAPIClient.CreateOrUpdateSecret(actionEnvironment, YouTubeConstants.ProjectApiKeyVariable, "empty", client.LogMessage);
            Console.WriteLine($"Paste Google API key in action secret {YouTubeConstants.ProjectApiKeyVariable}");
            canContinue = false;
        }
        if (canContinue && (!YouTubeAPIHelper.IsValidTokenResponse(actionInt.TokenResponse)))
        {
            Console.WriteLine("Entering Google API stored token response section");
            GoogleAuthorizationCodeFlow flow = YouTubeAPIHelper.GetFlow(actionInt.AppClientId, actionInt.AppClientSecret);
            if (!string.IsNullOrEmpty(workflow.RedirectCode) && (!workflow.RedirectCode.Equals("empty")))
            {
                using (HttpClient tokenHttpClient = new HttpClient())
                {
                    string redirectTokenCode = workflow.RedirectCode.Trim().Replace("=", "").Replace("&", "");
                    var authorizationCodeTokenRequest = YouTubeAPIHelper.GetTokenRequest(redirectTokenCode, actionInt.AppClientId, actionInt.AppClientSecret);
                    TokenResponse? authorizationCodeTokenResponse = YouTubeAPIHelper.ExchangeAuthorizationCode(authorizationCodeTokenRequest, flow);
                    if (authorizationCodeTokenResponse != null)
                    {
                        YouTubeAPIHelper.SaveTokenResponse(authorizationCodeTokenResponse, actionEnvironment, client.LogMessage);
                        Console.WriteLine($"Google API stored token response has been saved to {YouRataConstants.StoredTokenResponseVariable}");
                    }
                    else
                    {
                        Console.WriteLine("Could not exchange authorization code");
                        canContinue = false;
                    }
                }
            }
            else if (GitHubAPIClient.CreateOrUpdateSecret(actionEnvironment, YouRataConstants.StoredTokenResponseVariable, "empty", client.LogMessage))
            {
                Uri url = flow.CreateAuthorizationCodeRequest(GoogleAuthConsts.LocalhostRedirectUri).Build();
                GitHubAPIClient.CreateOrUpdateSecret(actionEnvironment, YouTubeConstants.RedirectCodeVariable, "empty", client.LogMessage);
                Console.WriteLine("Follow this link to authorize this GitHub application");
                Console.WriteLine(url);
                Console.WriteLine("When finished it is normal that the site can't be reached");
                Console.WriteLine("===============================================================");
                Console.WriteLine("Copy everything in the website URL between |code=| and |&|");
                Console.WriteLine($"Paste this value in action secret {YouTubeConstants.RedirectCodeVariable}");
                canContinue = false;
            }
            else
            {
                Console.WriteLine("Failed to create repository secrets");
                canContinue = false;
            }
        }
        if (!canContinue)
        {
            client.BlockAllMilestones();
        }
    }
    catch (Exception ex)
    {
        client.SetStatus(MilestoneCondition.MilestoneFailed);
        throw new MilestoneException("InitialSetup failed", ex);
    }
    client.SetStatus(MilestoneCondition.MilestoneCompleted);
}
