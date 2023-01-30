using YouRatta.ConflictMonitor.MilestoneCall;
using YouRatta.ConflictMonitor.MilestoneInterface;

CallHandler callHandler= new CallHandler();
WebAppServer appServer = new WebAppServer(callHandler);
appServer.RunAsync().Wait();
