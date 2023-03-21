using System;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
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
            Console.WriteLine($"{GitHubConstants.NoticeAnnotation}Create an action secret {GitHubConstants.ApiTokenVariable} to store GitHub personal access token");
            canContinue = false;
        }
        if (canContinue && (!Regex.IsMatch(actionEnvironment.ApiToken, @"^ghp_[a-zA-Z0-9]{36}$")))
        {
            Console.WriteLine("Entering actions secrets verify section");
            Console.WriteLine($"{GitHubConstants.ErrorAnnotation} secret {GitHubConstants.ApiTokenVariable} is not a GitHub personal access token");
            canContinue = false;
        }
        if (canContinue && ((string.IsNullOrEmpty(workflow.ProjectClientSecret) || string.IsNullOrEmpty(workflow.ProjectClientId)) ||
                            (workflow.ProjectClientSecret.Equals("empty") || workflow.ProjectClientId.Equals("empty"))))
        {
            Console.WriteLine("Entering actions variables section");
            if (!config.ActionCutOuts.DisableUnsupportedGitHubAPI)
            {
                UnsupportedGitHubAPIClient.CreateVariable(actionEnvironment, YouTubeConstants.ProjectClientIdVariable, "empty", client.LogMessage);
                client.Keepalive();
                UnsupportedGitHubAPIClient.CreateVariable(actionEnvironment, YouTubeConstants.ProjectClientSecretsVariable, "empty", client.LogMessage);
                client.Keepalive();
                Console.WriteLine($"{GitHubConstants.NoticeAnnotation}Fill repository variables and run action again");
            }
            else
            {
                Console.WriteLine($"{GitHubConstants.NoticeAnnotation}Create an action variable {YouTubeConstants.ProjectClientIdVariable} to store Google API client ID");
                Console.WriteLine($"{GitHubConstants.NoticeAnnotation}Create an action variable {YouTubeConstants.ProjectClientSecretsVariable} to store Google API client secrets");
            }
            canContinue = false;
        }
        if (canContinue && (string.IsNullOrEmpty(workflow.ProjectApiKey) || workflow.ProjectApiKey.Equals("empty")))
        {
            Console.WriteLine("Entering Google API key section");
            GitHubAPIClient.CreateOrUpdateSecret(actionEnvironment, YouTubeConstants.ProjectApiKeyVariable, "empty", client.LogMessage);
            client.Keepalive();
            Console.WriteLine($"{GitHubConstants.NoticeAnnotation}Paste Google API key in action secret {YouTubeConstants.ProjectApiKeyVariable}");
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
                    client.Keepalive();
                    if (authorizationCodeTokenResponse != null)
                    {
                        YouTubeAPIHelper.SaveTokenResponse(authorizationCodeTokenResponse, actionEnvironment, client.LogMessage);
                        client.Keepalive();
                        Console.WriteLine($"Google API stored token response has been saved to {YouRataConstants.StoredTokenResponseVariable}");
                    }
                    else
                    {
                        Console.WriteLine($"{GitHubConstants.ErrorAnnotation}Could not exchange authorization code");
                        canContinue = false;
                    }
                }
            }
            else if (GitHubAPIClient.CreateOrUpdateSecret(actionEnvironment, YouRataConstants.StoredTokenResponseVariable, "empty", client.LogMessage))
            {
                client.Keepalive();
                Uri url = flow.CreateAuthorizationCodeRequest(GoogleAuthConsts.LocalhostRedirectUri).Build();
                GitHubAPIClient.CreateOrUpdateSecret(actionEnvironment, YouTubeConstants.RedirectCodeVariable, "empty", client.LogMessage);
                client.Keepalive();
                StringBuilder authorizeInstructions = new StringBuilder();
                authorizeInstructions.Append(GitHubConstants.NoticeAnnotation);
                authorizeInstructions.Append("Follow this link to authorize this GitHub application");
                authorizeInstructions.Append("%0A");
                authorizeInstructions.Append(url.ToString());
                authorizeInstructions.Append("%0A");
                authorizeInstructions.Append("When finished it is normal that the site can't be reached");
                authorizeInstructions.Append("%0A");
                authorizeInstructions.Append("===============================================================");
                authorizeInstructions.Append("%0A");
                authorizeInstructions.Append("Copy everything in the website URL between |code=| and |&|");
                authorizeInstructions.Append("%0A");
                authorizeInstructions.Append("Paste this value in action secret ");
                authorizeInstructions.Append(YouTubeConstants.RedirectCodeVariable);
                Console.WriteLine(authorizeInstructions.ToString());
                canContinue = false;
            }
            else
            {
                client.Keepalive();
                Console.WriteLine($"{GitHubConstants.ErrorAnnotation}Failed to create repository secrets");
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
