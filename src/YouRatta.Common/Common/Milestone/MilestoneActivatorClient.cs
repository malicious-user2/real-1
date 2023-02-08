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
using Grpc.Core;

namespace YouRatta.Common.Milestone;

public abstract class MilestoneActivatorClient : IDisposable
{
    private readonly GrpcChannel _conflictMonitorChannel;
    private bool _disposed;

    public MilestoneActivatorClient()
    {
        _conflictMonitorChannel = GrpcChannel.ForAddress($"http://{IPAddress.Loopback}", new GrpcChannelOptions
        {
            HttpHandler = CreateHttpHandler(GrpcConstants.UnixSocketPath)
        });
    }

    private bool IsValidMilestoneIntelligenceType(System.Type milestoneIntelligence)
    {
        System.Type[] milestoneTypes = typeof(MilestoneActionIntelligence.Types).GetNestedTypes();
        if (!milestoneTypes.Contains(milestoneIntelligence)) return false;
        return true;
    }

    public virtual void SetStatus(MilestoneCondition milestoneCondition, System.Type milestoneIntelligenceType, string milestoneIntelligenceName)
    {
        if (!IsValidMilestoneIntelligenceType(milestoneIntelligenceType)) return;
        object? milestoneActionIntelligence = GetMilestoneActionIntelligence(milestoneIntelligenceType);
        if (milestoneActionIntelligence != null)
        {
            milestoneActionIntelligence.GetType().GetProperty("Condition")?.SetValue(milestoneActionIntelligence, milestoneCondition);

            SetMilestoneActionIntelligence(milestoneActionIntelligence, milestoneIntelligenceType, milestoneIntelligenceName);
        }
    }

    public virtual MilestoneCondition GetStatus(System.Type milestoneIntelligenceType)
    {
        MilestoneCondition milestoneCondition = new MilestoneCondition();
        object? milestoneActionIntelligence = GetMilestoneActionIntelligence(milestoneIntelligenceType);
        if (milestoneActionIntelligence != null)
        {
            PropertyInfo? conditionProperty = milestoneActionIntelligence.GetType().GetProperty("Condition");
            object? conditionValue = conditionProperty?.GetValue(milestoneActionIntelligence, null);
            if (conditionValue != null)
            {
                milestoneCondition = (MilestoneCondition)conditionValue;
            }

        }
        return milestoneCondition;
    }

    public virtual void SetMilestoneActionIntelligence(object milestoneActionIntelligence, System.Type milestoneIntelligenceType, string milestoneIntelligenceName)
    {
        if (!IsValidMilestoneIntelligenceType(milestoneIntelligenceType)) return;
        MilestoneActionIntelligenceServiceClient milestoneClient = new MilestoneActionIntelligenceServiceClient(_conflictMonitorChannel);
        List<MethodInfo> clientMethods = milestoneClient
            .GetType()
            .GetMethods()
            .Where(method => method
            .GetParameters().Length == 4 && method
            .Name == $"Update{milestoneIntelligenceName}ActionIntelligence")
            .ToList();
        foreach (MethodInfo clientMethod in clientMethods)
        {
            clientMethod.Invoke(milestoneClient, new object?[] { milestoneActionIntelligence, null, null, default(CancellationToken) });
            break;
        }
    }

    public virtual object? GetMilestoneActionIntelligence(System.Type milestoneIntelligenceType)
    {
        object? milestoneActionIntelligence = null;
        if (!IsValidMilestoneIntelligenceType(milestoneIntelligenceType)) return milestoneActionIntelligence;
        ActionIntelligenceServiceClient client = new ActionIntelligenceServiceClient(_conflictMonitorChannel);
        ActionIntelligence initialIntelligence = client.GetActionIntelligence(new Empty());
        List<PropertyInfo> initialIntelligenceProperties = initialIntelligence.MilestoneIntelligence
            .GetType()
            .GetProperties()
            .Where(prop => prop.PropertyType == milestoneIntelligenceType)
            .ToList();
        foreach (PropertyInfo initialIntelligenceProperty in initialIntelligenceProperties)
        {
            object? intelligenceClass = initialIntelligenceProperty.GetValue(initialIntelligence.MilestoneIntelligence);
            if (intelligenceClass != null)
            {
                milestoneActionIntelligence = intelligenceClass;
            }
            break;
        }
        return milestoneActionIntelligence;
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

        internal ValueTask<Stream> PlaintextFilter(SocketsHttpPlaintextStreamFilterContext filterContext, CancellationToken cancellationToken = default)
        {
            return new ValueTask<Stream>(Task.Run<Stream>(() => filterContext.PlaintextStream));
        }
    }
}
