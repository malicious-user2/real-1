using YouRatta.Common.Configurations;
using YouRatta.Common.GitHub;
using YouRatta.ConflictMonitor.MilestoneCall;
using YouRatta.ConflictMonitor.MilestoneInterface;
using YouRatta.ConflictMonitor.Workflow;

CallHandler callHandler = new CallHandler();
InServiceLoggerProvider logProvider = new InServiceLoggerProvider(callHandler);
ConfigurationHelper configurationHelper = new ConfigurationHelper(args);
GitHubEnvironmentHelper environmentHelper = new GitHubEnvironmentHelper();
ConflictMonitorWorkflow conflictMonitorWorkflow = new ConflictMonitorWorkflow();
WebAppServer appServer = new WebAppServer(callHandler, logProvider, configurationHelper, environmentHelper, conflictMonitorWorkflow);
appServer.RunAsync().Wait();
