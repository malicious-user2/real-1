using System;
using System.Diagnostics;
using Octokit;
using YouRatta.Common.GitHub;
using YouRatta.Common.Milestone;
using YouRatta.Common.Proto;
using YouRatta.InitialSetup.ConflictMonitor;
using static YouRatta.Common.Proto.MilestoneActionIntelligence.Types;

using (InitialSetupActivatorClient client = new InitialSetupActivatorClient())
{
    System.Threading.Thread.Sleep(3000);
    if (client.GetMilestoneActionIntelligence().Condition == MilestoneCondition.MilestoneBlocked) return;
    if (client.GetYouRattaConfiguration().ActionCutOuts.DisableInitialSetupMilestone) return;


    ActionIntelligence intel = client.GetActionIntelligence();


    GitHubClient ghClient = new GitHubClient(GitHubConstants.ProductHeader);
    ghClient.Credentials = new Credentials(intel.GitHubActionEnvironment.GitHubToken, AuthenticationType.Bearer);

    Connection conn2 = new Connection(GitHubConstants.ProductHeader);
    conn2.Credentials = new Credentials(intel.GitHubActionEnvironment.GitHubToken, AuthenticationType.Bearer);

    ApiConnection conn3 = new ApiConnection(conn2);
    RepositorySecretsClient cli = new RepositorySecretsClient(conn3);

    Console.WriteLine(intel.GitHubActionEnvironment.EnvGitHubRepositoryOwner);
    Console.WriteLine(intel.GitHubActionEnvironment.EnvGitHubRepository);
    cli.Delete(intel.GitHubActionEnvironment.EnvGitHubRepositoryOwner, intel.GitHubActionEnvironment.EnvGitHubRepository.Split("/")[1], "DELETEME").Wait();


    System.Console.WriteLine(client.GetYouRattaConfiguration().MilestoneLifetime.MaxRunTime);
    InitialSetupActionIntelligence milestoneActionIntelligence = new InitialSetupActionIntelligence();
    milestoneActionIntelligence.ProcessId = Process.GetCurrentProcess().Id;
    milestoneActionIntelligence.Condition = MilestoneCondition.MilestoneRunning;
    client.SetMilestoneActionIntelligence(milestoneActionIntelligence);
    System.Console.WriteLine(client.GetMilestoneActionIntelligence());
}

