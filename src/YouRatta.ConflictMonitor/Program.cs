using YouRatta.Common.Configurations;
using YouRatta.ConflictMonitor.MilestoneCall;
using YouRatta.ConflictMonitor.MilestoneInterface;

CallHandler callHandler = new CallHandler();
InServiceLoggerProvider logProvider = new InServiceLoggerProvider(callHandler);
ConfigurationHelper configurationHelper = new ConfigurationHelper(args);
WebAppServer appServer = new WebAppServer(callHandler, logProvider, configurationHelper);
appServer.RunAsync().Wait();
