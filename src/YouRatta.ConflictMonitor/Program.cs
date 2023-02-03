using YouRatta.Common.Configurations;
using YouRatta.Common.GitHub;
using YouRatta.ConflictMonitor.MilestoneCall;
using YouRatta.ConflictMonitor.MilestoneInterface;

CallHandler callHandler = new CallHandler();
InServiceLoggerProvider logProvider = new InServiceLoggerProvider(callHandler);
ConfigurationHelper configurationHelper = new ConfigurationHelper(args);
GitHubEnvironmentHelper environmentHelper = new GitHubEnvironmentHelper();
WebAppServer appServer = new WebAppServer(callHandler, logProvider, configurationHelper, environmentHelper);
appServer.RunAsync().Wait();
