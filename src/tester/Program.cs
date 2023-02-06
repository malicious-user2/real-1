// See https://aka.ms/new-console-template for more information
using Google.Protobuf;
using Grpc.Net.Client;
using System.Net;
using System.Net.Sockets;
using YouRatta.Common.Proto;
using static YouRatta.Common.Proto.ActionIntelligenceService;
using static YouRatta.Common.Proto.MilestoneActionIntelligenceService;

Console.WriteLine("Hello, World!");

var client_secrets = new YouRatta.Common.Proto.ClientSecrets();
var installed_client_secrets = new YouRatta.Common.Proto.InstalledClientSecrets();
installed_client_secrets.ClientId = "123";
client_secrets.InstalledClientSecrets = installed_client_secrets;
Console.WriteLine(JsonFormatter.Default.Format(client_secrets));


using var channel = GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions
{
    HttpHandler = CreateHttpHandler(YouRatta.ConflictMonitor.GrpcConstants.UnixSocketPath)
});

ActionIntelligenceServiceClient client = new ActionIntelligenceServiceClient(channel);
MilestoneActionIntelligenceServiceClient client2 = new MilestoneActionIntelligenceServiceClient(channel);

System.Threading.Thread.Sleep(7000);
var reply = await client.GetActionIntelligenceAsync(new Google.Protobuf.WellKnownTypes.Empty());
Console.WriteLine(reply.MilestoneIntelligence.InitialSetup.Condition);
var initialSetup = new MilestoneActionIntelligence.Types.InitialSetupActionIntelligence();
initialSetup.Condition = MilestoneActionIntelligence.Types.MilestoneCondition.MilestoneRunning;
client2.UpdateInitialSetupActionIntelligence(initialSetup);
var reply2 = await client.GetActionIntelligenceAsync(new Google.Protobuf.WellKnownTypes.Empty());
Console.WriteLine(reply2.MilestoneIntelligence.InitialSetup.Condition);

static SocketsHttpHandler CreateHttpHandler(string socketPath)
{
    var udsEndPoint = new UnixDomainSocketEndPoint(socketPath);
    var connectionFactory = new UnixDomainSocketConnectionFactory(udsEndPoint);
    var socketsHttpHandler = new SocketsHttpHandler
    {
        ConnectCallback = connectionFactory.ConnectAsync,
        PlaintextStreamFilter = connectionFactory.PlaintextFilter
    };

    return socketsHttpHandler;
}

public class UnixDomainSocketConnectionFactory
{
    private readonly EndPoint _endPoint;
    private string test;

    public UnixDomainSocketConnectionFactory(EndPoint endPoint)
    {
        _endPoint = endPoint;
        test = ":";
    }

    public async ValueTask<Stream> ConnectAsync(SocketsHttpConnectionContext _, CancellationToken cancellationToken = default)
    {
        var socket = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.Unspecified);

        try
        {
            await socket.ConnectAsync(_endPoint, cancellationToken).ConfigureAwait(false);
            test = "";
            return new NetworkStream(socket, true);
        }
        catch (Exception ex)
        {
            socket.Dispose();
            throw new HttpRequestException($"Error connecting to '{_endPoint}'.", ex);
        }
    }

    public async ValueTask<Stream> PlaintextFilter(SocketsHttpPlaintextStreamFilterContext dor, CancellationToken cancellationToken = default)
    {
        return dor.PlaintextStream;
    }
}
