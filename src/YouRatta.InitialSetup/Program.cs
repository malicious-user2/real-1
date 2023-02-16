using System;
using System.Diagnostics;
using System.Threading;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Requests;
using Google.Apis.YouTube.v3;
using Google.Protobuf;
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
                UnsupportedGitHubAPIClient.CreateVariable(actionEnvironment, YouTubeConstants.ClientIdVariable, "empty", client.LogMessage);
                UnsupportedGitHubAPIClient.CreateVariable(actionEnvironment, YouTubeConstants.ClientSecretsVariable, "empty", client.LogMessage);
                Console.WriteLine("Fill repository variables and run action again");
            }
            else
            {
                Console.WriteLine($"Create an action variable {YouTubeConstants.ClientIdVariable} to store Google API client ID");
                Console.WriteLine($"Create an action variable {YouTubeConstants.ClientSecretsVariable} to store Google API client secrets");
            }
            canContinue = false;
        }
        InstalledClientSecrets blankClientsecrets = new InstalledClientSecrets();
        if (canContinue && (intelligence.ClientSecrets.InstalledClientSecrets.Equals(blankClientsecrets)))
        {
            Console.WriteLine("Entering Google API stored client secrets section");
            if (GitHubAPIClient.CreateOrUpdateSecret(actionEnvironment, YouRattaConstants.StoredClientSecretsVariable, "empty", client.LogMessage))
            {
                GoogleAuthorizationCodeFlow flow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
                {
                    ClientSecrets = new Google.Apis.Auth.OAuth2.ClientSecrets()
                    {
                        ClientId = workflow.ClientId,
                        ClientSecret = workflow.ClientSecret
                    },
                    Scopes = new[] { YouTubeService.Scope.YoutubeForceSsl }
                });
                Uri url = flow.CreateAuthorizationCodeRequest(GoogleAuthConsts.LocalhostRedirectUri).Build();
                Console.WriteLine(flow.CreateAuthorizationCodeRequest(GoogleAuthConsts.LocalhostRedirectUri).Build());
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

