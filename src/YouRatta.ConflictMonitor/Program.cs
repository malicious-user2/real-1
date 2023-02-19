using YouRatta.Common.Configurations;
using YouRatta.Common.GitHub;
using YouRatta.ConflictMonitor.MilestoneData;
using YouRatta.ConflictMonitor.MilestoneCall;
using YouRatta.ConflictMonitor.MilestoneInterface;
using YouRatta.ConflictMonitor.Workflow;
using YouRatta.Common.YouTube;
using System;

CallHandler callHandler = new CallHandler();
InServiceLoggerProvider logProvider = new InServiceLoggerProvider(callHandler);
ConfigurationHelper configurationHelper = new ConfigurationHelper(args);
GitHubEnvironmentHelper environmentHelper = new GitHubEnvironmentHelper();
ConflictMonitorWorkflow conflictMonitorWorkflow = new ConflictMonitorWorkflow();
MilestoneIntelligenceRegistry milestoneIntelligence = new MilestoneIntelligenceRegistry();
if (YouTubeAPIHelper.IsValidTokenResponse(conflictMonitorWorkflow.StoredTokenResponse))
{
    conflictMonitorWorkflow.InitialSetupComplete = true;
}

WebAppServer appServer = new WebAppServer(callHandler, logProvider, configurationHelper, environmentHelper, conflictMonitorWorkflow, milestoneIntelligence);
appServer.RunAsync().Wait();
