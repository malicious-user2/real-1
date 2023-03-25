using System;
using System.Net.Http;
using System.Text.RegularExpressions;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Requests;
using Google.Apis.Auth.OAuth2.Responses;
using YouRata.Common;
using YouRata.Common.Configuration;
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
        GitHubWorkflowHelper.WriteStepSummary("# InitialSetup\n\n");
        if (string.IsNullOrEmpty(actionEnvironment.ApiToken))
        {
            Console.WriteLine("Entering actions secrets section");
            Announce($"Create an action secret {YouRataConstants.ApiTokenVariable} to store GitHub personal access token");
            GitHubWorkflowHelper.WriteStepSummary($"{actionEnvironment.EnvGitHubServerUrl}/{actionEnvironment.EnvGitHubRepository}/settings/secrets/actions\n");
            canContinue = false;
        }
        if (canContinue && (!Regex.IsMatch(actionEnvironment.ApiToken, @"^ghp_[a-zA-Z0-9]{36}$")))
        {
            Console.WriteLine("Entering actions secrets verify section");
            Announce($"Action secret {YouRataConstants.ApiTokenVariable} is not a GitHub personal access token");
            canContinue = false;
        }
        if (canContinue && ((string.IsNullOrEmpty(workflow.ProjectClientSecret) || string.IsNullOrEmpty(workflow.ProjectClientId)) ||
                            (workflow.ProjectClientSecret.Equals("empty") || workflow.ProjectClientId.Equals("empty"))))
        {
            Console.WriteLine("Entering actions variables section");
            if (!config.ActionCutOuts.DisableUnsupportedGitHubAPI)
            {
                UnsupportedGitHubAPIClient.CreateVariable(actionEnvironment, YouRataConstants.ProjectClientIdVariable, "empty", client.LogMessage);
                client.Keepalive();
                UnsupportedGitHubAPIClient.CreateVariable(actionEnvironment, YouRataConstants.ProjectClientSecretsVariable, "empty", client.LogMessage);
                client.Keepalive();
                Announce("Fill repository variables and run action again");
                GitHubWorkflowHelper.WriteStepSummary($"{actionEnvironment.EnvGitHubServerUrl}/{actionEnvironment.EnvGitHubRepository}/settings/variables/actions\n");
            }
            else
            {
                Announce($"Create an action variable {YouRataConstants.ProjectClientIdVariable} to store Google API client ID");
                Announce($"Create an action variable {YouRataConstants.ProjectClientSecretsVariable} to store Google API client secrets");
                GitHubWorkflowHelper.WriteStepSummary($"{actionEnvironment.EnvGitHubServerUrl}/{actionEnvironment.EnvGitHubRepository}/settings/variables/actions\n");
            }
            canContinue = false;
        }
        if (canContinue && (string.IsNullOrEmpty(workflow.ProjectApiKey) || workflow.ProjectApiKey.Equals("empty")))
        {
            Console.WriteLine("Entering Google API key section");
            GitHubAPIClient.CreateOrUpdateSecret(actionEnvironment, YouRataConstants.ProjectApiKeyVariable, "empty", client.LogMessage);
            client.Keepalive();
            Announce($"Paste Google API key in action secret {YouRataConstants.ProjectApiKeyVariable}");
            GitHubWorkflowHelper.WriteStepSummary($"{actionEnvironment.EnvGitHubServerUrl}/{actionEnvironment.EnvGitHubRepository}/settings/secrets/actions/{YouRataConstants.ProjectApiKeyVariable}\n");
            canContinue = false;
        }
        if (canContinue && (!YouTubeAPIHelper.GetTokenResponse(workflow.StoredTokenResponse, out _)))
        {
            Console.WriteLine("Entering Google API stored token response section");
            GoogleAuthorizationCodeFlow flow = YouTubeAPIHelper.GetFlow(workflow.ProjectClientId, workflow.ProjectClientSecret);
            if (!string.IsNullOrEmpty(workflow.RedirectCode) && workflow.RedirectCode != "empty")
            {
                string redirectTokenCode = workflow.RedirectCode.Trim().Replace("=", "").Replace("&", "");
                AuthorizationCodeTokenRequest authorizationCodeTokenRequest = YouTubeAPIHelper.GetTokenRequest(redirectTokenCode, workflow.ProjectClientId, workflow.ProjectClientSecret);
                TokenResponse? authorizationCodeTokenResponse = YouTubeAPIHelper.ExchangeAuthorizationCode(authorizationCodeTokenRequest, flow);
                client.Keepalive();
                if (authorizationCodeTokenResponse != null)
                {
                    YouTubeAPIHelper.SaveTokenResponse(authorizationCodeTokenResponse, actionEnvironment, client.LogMessage);
                    client.Keepalive();
                    Announce($"Google API stored token response has been saved to {YouRataConstants.StoredTokenResponseVariable}");
                    workflow.CopyDirectionsReadme = true;
                }
                else
                {
                    Console.WriteLine("Could not exchange authorization code");
                    canContinue = false;
                }
            }
            else if (GitHubAPIClient.CreateOrUpdateSecret(actionEnvironment, YouRataConstants.StoredTokenResponseVariable, "empty", client.LogMessage))
            {
                client.Keepalive();
                Uri url = flow.CreateAuthorizationCodeRequest(GoogleAuthConsts.LocalhostRedirectUri).Build();
                GitHubAPIClient.CreateOrUpdateSecret(actionEnvironment, YouRataConstants.RedirectCodeVariable, "empty", client.LogMessage);
                client.Keepalive();
                Announce("Follow this link to authorize this GitHub application");
                Announce(url.ToString());
                Announce("<ins>When finished it is normal that the site can't be reached</ins>");
                Announce("Copy everything in the website URL between |code=| and |&|");
                Announce($"Paste this value in action secret {YouRataConstants.RedirectCodeVariable}");
                GitHubWorkflowHelper.WriteStepSummary($"{actionEnvironment.EnvGitHubServerUrl}/{actionEnvironment.EnvGitHubRepository}/settings/secrets/actions/{YouRataConstants.RedirectCodeVariable}\n");
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

void Announce(string announcement)
{
    Console.WriteLine(announcement);
    GitHubWorkflowHelper.WriteStepSummary($"{announcement}\n");
}
