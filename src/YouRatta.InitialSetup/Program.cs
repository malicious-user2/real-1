using System;
using System.Diagnostics;
using System.Threading;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Util.Store;
using Google.Apis.YouTube.v3;
using Microsoft.Extensions.Hosting;
using Octokit;
using YouRatta.Common.GitHub;
using YouRatta.Common.Milestone;
using YouRatta.Common.Proto;
using YouRatta.InitialSetup.ConflictMonitor;
using YouRatta.InitialSetup.Workflow;
using static Google.Apis.Auth.OAuth2.Web.AuthorizationCodeWebApp;
using static YouRatta.Common.Proto.MilestoneActionIntelligence.Types;

using (InitialSetupCommunicationClient client = new InitialSetupCommunicationClient())
{
    System.Threading.Thread.Sleep(2000);
    if (client.GetMilestoneActionIntelligence().Condition == MilestoneCondition.MilestoneBlocked) return;
    if (client.GetYouRattaConfiguration().ActionCutOuts.DisableInitialSetupMilestone) return;



    Console.WriteLine(client.GetActionIntelligence());

    ActionIntelligence intel = client.GetActionIntelligence();

    InitialSetupWorkflow workflow = new InitialSetupWorkflow();



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


    UnsupportedGitHubAPIClient.CreateVariable(intel.GitHubActionEnvironment, "OTHERCREATE", "testval", client.LogMessage);



    System.Console.WriteLine(client.GetYouRattaConfiguration().MilestoneLifetime.MaxRunTime);
    InitialSetupActionIntelligence milestoneActionIntelligence = new InitialSetupActionIntelligence();
    milestoneActionIntelligence.ProcessId = Process.GetCurrentProcess().Id;
    milestoneActionIntelligence.Condition = MilestoneCondition.MilestoneRunning;
    client.SetMilestoneActionIntelligence(milestoneActionIntelligence);
    System.Console.ReadLine();
}

