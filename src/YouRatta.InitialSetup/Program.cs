using System;
using System.Diagnostics;
using System.Threading;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.YouTube.v3;
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
    client.Activate();
    Console.WriteLine($"Entering {client.MilestoneName}");
    try
    {
        bool canContinue = true;
        if (string.IsNullOrEmpty(workflow.ClientSecret) || string.IsNullOrEmpty(workflow.ClientId))
        {
            Console.WriteLine("Entering actions secrets and variables section");
            UnsupportedGitHubAPIClient.CreateVariable(actionEnvironment, YouTubeConstants.ClientIdVariable, "empty", client.LogMessage);
            UnsupportedGitHubAPIClient.CreateVariable(actionEnvironment, YouTubeConstants.ClientSecretsVariable, "empty", client.LogMessage);
            
        }



        Console.WriteLine(client.GetActionIntelligence());



        GoogleAuthorizationCodeFlow flow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
        {
            ClientSecrets = new Google.Apis.Auth.OAuth2.ClientSecrets()
            {
                //Dummy values
                ClientId = workflow.ClientId,
                ClientSecret = workflow.ClientSecret
            },
            Scopes = new[] { YouTubeService.Scope.YoutubeForceSsl }
        });

        Console.WriteLine(flow.CreateAuthorizationCodeRequest("https://localhost").Build());


    }
    catch (Exception)
    {
        client.SetStatus(MilestoneCondition.MilestoneFailed);
        throw;
    }
    client.SetStatus(MilestoneCondition.MilestoneCompleted);
}

