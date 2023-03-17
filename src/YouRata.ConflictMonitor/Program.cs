using System;
using YouRata.Common.Configurations;
using YouRata.Common.GitHub;
using YouRata.Common.YouTube;
using YouRata.ConflictMonitor.ActionReport;
using YouRata.ConflictMonitor.MilestoneCall;
using YouRata.ConflictMonitor.MilestoneData;
using YouRata.ConflictMonitor.MilestoneInterface;
using YouRata.ConflictMonitor.Workflow;

CallHandler callHandler = new CallHandler();
InServiceLoggerProvider logProvider = new InServiceLoggerProvider(callHandler);
ConfigurationHelper configurationHelper = new ConfigurationHelper(args);
GitHubEnvironmentHelper environmentHelper = new GitHubEnvironmentHelper();
ConflictMonitorWorkflow conflictMonitorWorkflow = new ConflictMonitorWorkflow();
MilestoneIntelligenceRegistry milestoneIntelligence = new MilestoneIntelligenceRegistry();
PreviousActionReportProvider actionReportProvider = new PreviousActionReportProvider();
if (YouTubeAPIHelper.IsValidTokenResponse(conflictMonitorWorkflow.StoredTokenResponse))
{
    conflictMonitorWorkflow.InitialSetupComplete = true;
}

WebAppServer appServer = new WebAppServer(callHandler, logProvider, configurationHelper, environmentHelper, conflictMonitorWorkflow, milestoneIntelligence, actionReportProvider);
appServer.RunAsync().Wait();
