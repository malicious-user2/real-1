using System;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Threading;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Requests;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.YouTube.v3;
using Google.Protobuf;
using Microsoft.VisualBasic;
using Newtonsoft.Json;
using YouRatta.Common;
using YouRatta.Common.Configurations;
using YouRatta.Common.GitHub;
using YouRatta.Common.Proto;
using YouRatta.Common.YouTube;
using YouRatta.InitialSetup.ConflictMonitor;
using YouRatta.InitialSetup.Workflow;
using static YouRatta.Common.Proto.MilestoneActionIntelligence.Types;

using (InitialSetupCommunicationClient client = new InitialSetupCommunicationClient())
{
    System.Threading.Thread.Sleep(2000);
    if (client.GetMilestoneActionIntelligence().Condition == MilestoneCondition.MilestoneBlocked) return;
    if (client.GetYouRattaConfiguration().ActionCutOuts.DisableInitialSetupMilestone) return;
    InitialSetupWorkflow workflow = new InitialSetupWorkflow();



    ActionIntelligence intelligence = client.GetActionIntelligence();
    GitHubActionEnvironment actionEnvironment = intelligence.GitHubActionEnvironment;
    YouRattaConfiguration config = client.GetYouRattaConfiguration();
    try
    {
        client.Activate();
        bool canContinue = true;
        if (string.IsNullOrEmpty(actionEnvironment.ApiToken))
        {
            Console.WriteLine("Entering actions secrets section");
            Console.WriteLine($"Create an action secret {GitHubConstants.ApiTokenVariable} to store GitHub personal access token");
            canContinue = false;
        }
        if (canContinue && ((string.IsNullOrEmpty(workflow.ClientSecret) || string.IsNullOrEmpty(workflow.ClientId)) ||
                            (workflow.ClientSecret.Equals("empty") || workflow.ClientId.Equals("empty"))))
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
        if (canContinue && (string.IsNullOrEmpty(intelligence.TokenResponse)))
        {
            Console.WriteLine("Entering Google API stored token response section");
            GoogleAuthorizationCodeFlow flow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
            {
                ClientSecrets = new Google.Apis.Auth.OAuth2.ClientSecrets()
                {
                    ClientId = workflow.ClientId,
                    ClientSecret = workflow.ClientSecret
                },
                Scopes = new[] { YouTubeService.Scope.YoutubeForceSsl }
            });
            if (!string.IsNullOrEmpty(workflow.RedirectCode) && (!workflow.RedirectCode.Equals("empty")))
            {
                using (HttpClient tokenHttpClient = new HttpClient())
                {
                    string redirectTokenCode = workflow.RedirectCode.Trim().Replace("=", "").Replace("&", "");
                    AuthorizationCodeTokenRequest authorizationCodeTokenRequest = new AuthorizationCodeTokenRequest
                    {
                        Scope = YouTubeService.Scope.YoutubeForceSsl,
                        RedirectUri = GoogleAuthConsts.LocalhostRedirectUri,
                        Code = redirectTokenCode,
                        ClientId = workflow.ClientId,
                        ClientSecret = workflow.ClientSecret
                    };
                    TokenResponse authorizationCodeTokenResponse = authorizationCodeTokenRequest.ExecuteAsync(tokenHttpClient, flow.TokenServerUrl, CancellationToken.None, flow.Clock).Result;
                    if (authorizationCodeTokenResponse != null)
                    {
                        string clientSecretsString = JsonConvert.SerializeObject(authorizationCodeTokenResponse, Formatting.None);
                        GitHubAPIClient.CreateOrUpdateSecret(actionEnvironment, YouRattaConstants.StoredTokenResponseVariable, clientSecretsString, client.LogMessage);
                        GitHubAPIClient.DeleteSecret(actionEnvironment, YouTubeConstants.RedirectCodeVariable, client.LogMessage);
                        Console.WriteLine($"Google API stored token response has been saved to {YouRattaConstants.StoredTokenResponseVariable}");
                    }
                    else
                    {
                        Console.WriteLine("Could not exchange authorization code");
                    }
                }

                
            }
            else if (GitHubAPIClient.CreateOrUpdateSecret(actionEnvironment, YouRattaConstants.StoredTokenResponseVariable, "empty", client.LogMessage))
            {
                Uri url = flow.CreateAuthorizationCodeRequest(GoogleAuthConsts.LocalhostRedirectUri).Build();
                GitHubAPIClient.CreateOrUpdateSecret(actionEnvironment, YouTubeConstants.RedirectCodeVariable, "empty", client.LogMessage);
                Console.WriteLine("Follow this link to authorize this GitHub application");
                Console.WriteLine(flow.CreateAuthorizationCodeRequest(GoogleAuthConsts.LocalhostRedirectUri).Build());
                Console.WriteLine("When finished it is normal that the site can't be reached");
                Console.WriteLine("===============================================================");
                Console.WriteLine("Copy everything in the browsers URL between |code=| and |&|");
                Console.WriteLine($"Paste this value in action secret {YouTubeConstants.RedirectCodeVariable}");
            }
            else
            {
                Console.WriteLine("Failed to create repository secrets");
            }
            canContinue = false;
        }



        Console.WriteLine(client.GetActionIntelligence());






    }
    catch (Exception)
    {
        client.SetStatus(MilestoneCondition.MilestoneFailed);
        throw;
    }
    client.SetStatus(MilestoneCondition.MilestoneCompleted);
}

