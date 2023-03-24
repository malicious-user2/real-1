// See https://aka.ms/new-console-template for more information
using Google.Protobuf;
using Grpc.Net.Client;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using YouRata.Common;
using YouRata.Common.Proto;
using static YouRata.Common.Proto.ActionIntelligenceService;
using static YouRata.Common.Proto.MilestoneActionIntelligenceService;

Console.WriteLine("Hello, World!");


using var channel = GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions
{
    HttpHandler = CreateHttpHandler(YouRataConstants.GrpcUnixSocketPath)
});

ActionIntelligenceServiceClient client = new ActionIntelligenceServiceClient(channel);
MilestoneActionIntelligenceServiceClient client2 = new MilestoneActionIntelligenceServiceClient(channel);

System.Threading.Thread.Sleep(7000);
var reply = await client.GetActionIntelligenceAsync(new Google.Protobuf.WellKnownTypes.Empty());
Console.WriteLine(reply.MilestoneIntelligence.InitialSetup.Condition);
var initialSetup = new MilestoneActionIntelligence.Types.InitialSetupActionIntelligence();
initialSetup.Condition = MilestoneActionIntelligence.Types.MilestoneCondition.MilestoneRunning;
initialSetup.ProcessId = Process.GetCurrentProcess().Id;
client2.UpdateInitialSetupActionIntelligence(initialSetup);
System.Threading.Thread.Sleep(8000);
client2.UpdateInitialSetupActionIntelligence(initialSetup);
var reply2 = await client.GetActionIntelligenceAsync(new Google.Protobuf.WellKnownTypes.Empty());
Console.WriteLine(reply2.MilestoneIntelligence.InitialSetup.Condition);
Console.ReadLine();

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
    private readonly string test;

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
