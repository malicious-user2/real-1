using YouRatta.ConflictMonitor.MilestoneCall;
using YouRatta.ConflictMonitor.MilestoneInterface;

CallHandler callHandler= new CallHandler();
InServiceLoggerProvider logProvider= new InServiceLoggerProvider(callHandler);
WebAppServer appServer = new WebAppServer(callHandler, logProvider);
appServer.RunAsync().Wait();
