using System;
using System.Net.Http;
using System.Text.RegularExpressions;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Requests;
using Google.Apis.Auth.OAuth2.Responses;
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
    if (!client.Activate(out InitialSetupActionIntelligence milestoneInt)) return;
    if (client.GetYouRataConfiguration().ActionCutOuts.DisableInitialSetupMilestone) return;
    MilestoneVariablesHelper.CreateRuntimeVariables(client, out ActionIntelligence actionInt, out YouRataConfiguration config, out GitHubActionEnvironment actionEnvironment);
    InitialSetupWorkflow workflow = new InitialSetupWorkflow();
    try
    {
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
        if (canContinue && ((string.IsNullOrEmpty(workflow.ProjectClientSecret) || string.IsNullOrEmpty(workflow.ProjectClientId)) ||
                            (workflow.ProjectClientSecret.Equals("empty") || workflow.ProjectClientId.Equals("empty"))))
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
        if (canContinue && (string.IsNullOrEmpty(workflow.ProjectApiKey) || workflow.ProjectApiKey.Equals("empty")))
        {
            Console.WriteLine("Entering Google API key section");
            GitHubAPIClient.CreateOrUpdateSecret(actionEnvironment, YouTubeConstants.ProjectApiKeyVariable, "empty", client.LogMessage);
            Console.WriteLine($"Paste Google API key in action secret {YouTubeConstants.ProjectApiKeyVariable}");
            canContinue = false;
        }
        if (canContinue && (!YouTubeAPIHelper.GetTokenResponse(workflow.StoredTokenResponse, out _)))
        {
            Console.WriteLine("Entering Google API stored token response section");
            GoogleAuthorizationCodeFlow flow = YouTubeAPIHelper.GetFlow(workflow.ProjectClientId, workflow.ProjectClientSecret);
            if (!string.IsNullOrEmpty(workflow.RedirectCode) && workflow.RedirectCode != "empty")
            {
                using (HttpClient tokenHttpClient = new HttpClient())
                {
                    string redirectTokenCode = workflow.RedirectCode.Trim().Replace("=", "").Replace("&", "");
                    AuthorizationCodeTokenRequest authorizationCodeTokenRequest = YouTubeAPIHelper.GetTokenRequest(redirectTokenCode, workflow.ProjectClientId, workflow.ProjectClientSecret);
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
