using System;
using System.Diagnostics;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Util.Store;
using Octokit;
using YouRatta.Common.GitHub;
using YouRatta.Common.Milestone;
using YouRatta.Common.Proto;
using YouRatta.InitialSetup.ConflictMonitor;
using static YouRatta.Common.Proto.MilestoneActionIntelligence.Types;

using (InitialSetupCommunicationClient client = new InitialSetupCommunicationClient())
{
    System.Threading.Thread.Sleep(2000);
    if (client.GetMilestoneActionIntelligence().Condition == MilestoneCondition.MilestoneBlocked) return;
    if (client.GetYouRattaConfiguration().ActionCutOuts.DisableInitialSetupMilestone) return;



    Console.WriteLine(client.GetActionIntelligence());

    ActionIntelligence intel = client.GetActionIntelligence();

    IAuthorizationCodeFlow flow =
        new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
        {
            ClientSecrets = new Google.Apis.Auth.OAuth2.ClientSecrets
            {
                ClientId = "PUT_CLIENT_ID_HERE",
                ClientSecret = "PUT_CLIENT_SECRET_HERE"
            },
            
            DataStore = new FileDataStore("Drive.Api.Auth.Store")
        });
    Google.Apis.Auth.OAuth2.Web.AuthorizationCodeWebApp webapp = new Google.Apis.Auth.OAuth2.Web.AuthorizationCodeWebApp()

System.Console.WriteLine(client.GetYouRattaConfiguration().MilestoneLifetime.MaxRunTime);
    InitialSetupActionIntelligence milestoneActionIntelligence = new InitialSetupActionIntelligence();
    milestoneActionIntelligence.ProcessId = Process.GetCurrentProcess().Id;
    milestoneActionIntelligence.Condition = MilestoneCondition.MilestoneRunning;
    client.SetMilestoneActionIntelligence(milestoneActionIntelligence);
    System.Console.ReadLine();
}

