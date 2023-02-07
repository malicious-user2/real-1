using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Threading;
using Grpc.Net.Client;
using static YouRatta.Common.Proto.MilestoneActionIntelligence.Types;
using Microsoft.AspNetCore.Http;
using static System.Net.Mime.MediaTypeNames;
using System.Net.Sockets;
using YouRatta.ConflictMonitor;
using static YouRatta.Common.Proto.ActionIntelligenceService;
using Google.Protobuf.WellKnownTypes;
using YouRatta.Common.Proto;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using Google.Protobuf;
using static YouRatta.Common.Proto.MilestoneActionIntelligenceService;

namespace YouRatta.Common.Milestone;

public abstract class MilestoneActivatorClient : IDisposable
{
    private readonly MilestoneIdentity _milestoneIdentity;
    private readonly GrpcChannel _conflictMonitorChannel;
    private DateTimeOffset _milestoneStarted;
    private bool _started;
    private bool _disposed;

    public MilestoneActivatorClient(string milestoneName)
    {
        _milestoneIdentity = new MilestoneIdentity { MilestoneName = milestoneName };
        _conflictMonitorChannel = GrpcChannel.ForAddress($"http://{IPAddress.Loopback}", new GrpcChannelOptions
        {
            HttpHandler = CreateHttpHandler(GrpcConstants.UnixSocketPath)
        });
    }

    private bool CheckValidMilestoneType(System.Type milestoneIntelligence)
    {
        System.Type[] milestoneTypes = typeof(MilestoneActionIntelligence.Types).GetNestedTypes();
        if (!milestoneTypes.Contains(milestoneIntelligence)) return false;
        return true;
    }

    public virtual void SetStatus(MilestoneCondition status, System.Type milestoneIntelligence, string milestoneIntelligenceName)
    {
        if (!CheckValidMilestoneType(milestoneIntelligence)) return;
        if (!_started)
        {
            _milestoneStarted = DateTimeOffset.Now;
            _started = true;
        }
        MilestoneActionIntelligenceServiceClient milestoneClient = new MilestoneActionIntelligenceServiceClient(_conflictMonitorChannel);
        List<MethodInfo> clientMethods = milestoneClient
            .GetType()
            .GetMethods()
            .Where(method => method.Name == $"Update{milestoneIntelligenceName}ActionIntelligence")
            .ToList();
        foreach (MethodInfo clientMethod in clientMethods)
        {
            ActionIntelligenceServiceClient actionClient = new ActionIntelligenceServiceClient(_conflictMonitorChannel);
            ActionIntelligence initialIntelligence = actionClient.GetActionIntelligence(new Empty());
            List<PropertyInfo> initialIntelligenceProperties = initialIntelligence.MilestoneIntelligence
                .GetType()
                .GetProperties()
                .Where(prop => prop.PropertyType == milestoneIntelligence)
                .ToList();
            foreach (PropertyInfo initialIntelligenceProperty in initialIntelligenceProperties)
            {
                object? intelligenceClass = initialIntelligenceProperty.GetValue(initialIntelligence.MilestoneIntelligence);
                if (intelligenceClass != null)
                {
                    intelligenceClass.GetType().GetProperty("Condition")?.SetValue(intelligenceClass, status);

                    clientMethod.Invoke(milestoneClient, new object[] { intelligenceClass });

                }
            }
        }
    }

    public MilestoneCondition GetStatus(System.Type milestoneIntelligence)
    {
        MilestoneCondition status = new MilestoneCondition();
        if (!CheckValidMilestoneType(milestoneIntelligence)) return status;
        ActionIntelligenceServiceClient client = new ActionIntelligenceServiceClient(_conflictMonitorChannel);
        ActionIntelligence initialIntelligence = client.GetActionIntelligence(new Empty());
        List<PropertyInfo> initialIntelligenceProperties = initialIntelligence.MilestoneIntelligence
            .GetType()
            .GetProperties()
            .Where(prop => prop.PropertyType == milestoneIntelligence)
            .ToList();
        foreach (PropertyInfo initialIntelligenceProperty in initialIntelligenceProperties)
        {
            object? intelligenceClass = initialIntelligenceProperty.GetValue(initialIntelligence.MilestoneIntelligence);
            if (intelligenceClass != null)
            {
                status = (MilestoneCondition)intelligenceClass.GetType().GetProperty("Condition").GetValue(intelligenceClass, null);
            }
        }
        return status;

    }

    private static SocketsHttpHandler CreateHttpHandler(string socketPath)
    {
        UnixDomainSocketEndPoint endPoint = new UnixDomainSocketEndPoint(socketPath);
        UnixDomainSocketConnectionFactory connectionFactory = new UnixDomainSocketConnectionFactory(endPoint);
        SocketsHttpHandler socketsHttpHandler = new SocketsHttpHandler
        {
            ConnectCallback = connectionFactory.ConnectAsync,
            PlaintextStreamFilter = connectionFactory.PlaintextFilter
        };

        return socketsHttpHandler;
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _conflictMonitorChannel.Dispose();
            _disposed = true;
        }
    }

    public MilestoneIdentity MilestoneId => _milestoneIdentity;

    private class UnixDomainSocketConnectionFactory
    {
        private readonly EndPoint _endPoint;

        internal UnixDomainSocketConnectionFactory(EndPoint endPoint)
        {
            _endPoint = endPoint;
        }

        internal async ValueTask<Stream> ConnectAsync(SocketsHttpConnectionContext _, CancellationToken cancellationToken = default)
        {
            Socket unixSocket = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.Unspecified);

            try
            {
                await unixSocket.ConnectAsync(_endPoint, cancellationToken).ConfigureAwait(false);
                return new NetworkStream(unixSocket, true);
            }
            catch (Exception ex)
            {
                unixSocket.Dispose();
                throw new HttpRequestException($"Error connecting to '{_endPoint}'.", ex);
            }
        }

        internal async ValueTask<Stream> PlaintextFilter(SocketsHttpPlaintextStreamFilterContext dor, CancellationToken cancellationToken = default)
        {
            return dor.PlaintextStream;
        }
    }
}
