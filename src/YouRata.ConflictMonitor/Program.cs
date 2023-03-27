// Copyright (c) 2023 battleship-systems.
// Licensed under the MIT license.

using System;
using System.Linq;
using YouRata.Common.Configuration;
using YouRata.Common.GitHub;
using YouRata.ConflictMonitor.ActionReport;
using YouRata.ConflictMonitor.MilestoneCall;
using YouRata.ConflictMonitor.MilestoneData;
using YouRata.ConflictMonitor.MilestoneInterface;
using YouRata.ConflictMonitor.Workflow;

/// <summary>
/// Create runtime objects and start the WebApp
/// </summary>
/// <remarks>
/// This WebApp is started in the background before any milestones
/// The WebApp provides information to the milestones on request and collects action intelligence
/// If it becomes necessary to kill an inert process ConflictMonitor retains its last intelligence
/// </remarks>

CallHandler callHandler = new CallHandler();
InServiceLoggerProvider logProvider = new InServiceLoggerProvider(callHandler);
ConfigurationHelper configurationHelper = new ConfigurationHelper(args);
GitHubEnvironmentHelper environmentHelper = new GitHubEnvironmentHelper();
ConflictMonitorWorkflow conflictMonitorWorkflow = new ConflictMonitorWorkflow();
MilestoneIntelligenceRegistry milestoneIntelligence = new MilestoneIntelligenceRegistry();
PreviousActionReportProvider actionReportProvider = new PreviousActionReportProvider();
WebAppServer appServer = new WebAppServer(callHandler, logProvider, configurationHelper, environmentHelper, conflictMonitorWorkflow,
    milestoneIntelligence, actionReportProvider);
WorkflowLogicProvider.ProcessWorkflow(conflictMonitorWorkflow);

if (args?.FirstOrDefault() == "newconfig")
{
    // Create a blank config file and exit
    configurationHelper.Build();
}
else
{
    // Run the WebApp until stopped
    appServer.RunAsync().Wait();
}
