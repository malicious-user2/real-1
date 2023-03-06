using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Grpc.Net.Client;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using YouRata.Common.ActionReport;
using YouRata.Common.Configurations;
using YouRata.Common.Proto;
using YouRata.ConflictMonitor;
using static YouRata.Common.Proto.ActionIntelligenceService;
using static YouRata.Common.Proto.MilestoneActionIntelligence.Types;
using static YouRata.Common.Proto.MilestoneActionIntelligenceService;
using static YouRata.Common.Proto.MilestoneLogService;

namespace YouRata.Common.Milestone;

public abstract class MilestoneCommunicationClient : IDisposable
{
    private readonly GrpcChannel _conflictMonitorChannel;
    private bool _disposed;

    protected MilestoneCommunicationClient()
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

    public virtual void LogMessage(string message, string milestone)
    {
        MilestoneLogServiceClient milestoneLogServiceClient = new MilestoneLogServiceClient(_conflictMonitorChannel);
        MilestoneLog milestoneLog = new MilestoneLog();
        milestoneLog.Message = message;
        milestoneLog.Milestone = milestone;
        milestoneLogServiceClient.WriteLogMessage(milestoneLog);
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
        MilestoneActionIntelligenceServiceClient milestoneActionIntelligenceServiceClient = new MilestoneActionIntelligenceServiceClient(_conflictMonitorChannel);
        List<MethodInfo> clientMethods = milestoneActionIntelligenceServiceClient
            .GetType()
            .GetMethods()
            .Where(method => method
                .GetParameters().Length == 4 && method
                .Name == $"Update{milestoneIntelligenceName}ActionIntelligence")
            .ToList();
        if (clientMethods != null && clientMethods.Count > 0)
        {
            MethodInfo clientMethod = clientMethods.First();
            clientMethod.Invoke(milestoneActionIntelligenceServiceClient, new object?[] { milestoneActionIntelligence, null, null, default(CancellationToken) });
        }
    }

    public virtual object? GetMilestoneActionIntelligence(System.Type milestoneIntelligenceType)
    {
        object? milestoneActionIntelligence = null;
        if (!IsValidMilestoneIntelligenceType(milestoneIntelligenceType)) return milestoneActionIntelligence;
        ActionIntelligenceServiceClient actionIntelligenceServiceClient = new ActionIntelligenceServiceClient(_conflictMonitorChannel);
        ActionIntelligence? actionIntelligence = null;
        try
        {
            actionIntelligence = actionIntelligenceServiceClient.GetActionIntelligence(new Empty());
        }
        catch (Grpc.Core.RpcException ex)
        {
            throw new MilestoneException("Failed to connect to ConflictMonitor", ex);
        }
        if (actionIntelligence == null) return milestoneActionIntelligence;
        List<PropertyInfo> milestoneIntelligenceProperties = actionIntelligence.MilestoneIntelligence
            .GetType()
            .GetProperties()
            .Where(prop => prop.PropertyType == milestoneIntelligenceType)
            .ToList();
        if (milestoneIntelligenceProperties != null && milestoneIntelligenceProperties.Count > 0)
        {
            PropertyInfo milestoneIntelligenceProperty = milestoneIntelligenceProperties.First();
            object? milestoneIntelligenceObject = milestoneIntelligenceProperty.GetValue(actionIntelligence.MilestoneIntelligence);
            if (milestoneIntelligenceObject != null)
            {
                milestoneActionIntelligence = milestoneIntelligenceObject;
            }
        }
        return milestoneActionIntelligence;
    }

    public void BlockAllMilestones()
    {
        ActionIntelligenceServiceClient actionIntelligenceServiceClient = new ActionIntelligenceServiceClient(_conflictMonitorChannel);
        ActionIntelligence actionIntelligence = actionIntelligenceServiceClient.GetActionIntelligence(new Empty());
        List<PropertyInfo> milestoneIntelligenceProperties = actionIntelligence.MilestoneIntelligence
            .GetType()
            .GetProperties()
            .Where(prop => prop.PropertyType.Name.Contains("ActionIntelligence"))
            .ToList();
        if (milestoneIntelligenceProperties != null && milestoneIntelligenceProperties.Count > 0)
        {
            foreach (PropertyInfo milestoneIntelligenceProperty in milestoneIntelligenceProperties)
            {
                object? milestoneIntelligenceObject = milestoneIntelligenceProperty.GetValue(actionIntelligence.MilestoneIntelligence);
                if (milestoneIntelligenceObject != null)
                {
                    string milestoneIntelligenceName = milestoneIntelligenceProperty.Name;
                    System.Type milestoneIntelligenceType = milestoneIntelligenceObject.GetType();
                    PropertyInfo? conditionProperty = milestoneIntelligenceType.GetProperty("Condition");
                    if (conditionProperty == null)
                    {
                        continue;
                    }
                    Object? conditionCurrentValue = conditionProperty.GetValue(milestoneIntelligenceObject);
                    if (conditionCurrentValue != null && conditionCurrentValue is MilestoneCondition)
                    {
                        MilestoneCondition currentCondition = (MilestoneCondition)conditionCurrentValue;
                        if (currentCondition == MilestoneCondition.MilestoneCompleted ||
                            currentCondition == MilestoneCondition.MilestoneFailed)
                        {
                            continue;
                        }
                    }
                    conditionProperty.SetValue(milestoneIntelligenceObject, MilestoneCondition.MilestoneBlocked);

                    SetMilestoneActionIntelligence(milestoneIntelligenceObject, milestoneIntelligenceType, milestoneIntelligenceName);
                }
            }
        }
    }

    public ActionIntelligence GetActionIntelligence()
    {
        ActionIntelligenceServiceClient actionIntelligenceServiceClient = new ActionIntelligenceServiceClient(_conflictMonitorChannel);
        return actionIntelligenceServiceClient.GetActionIntelligence(new Empty());
    }

    public ActionReportLayout GetPreviousActionReport()
    {
        ActionIntelligenceServiceClient actionIntelligenceServiceClient = new ActionIntelligenceServiceClient(_conflictMonitorChannel);
        string actionReportText = actionIntelligenceServiceClient.GetActionIntelligence(new Empty()).PreviousActionReport;
        Console.WriteLine(actionReportText);
        ActionReportLayout actionReport = new ActionReportLayout();
        if (!string.IsNullOrEmpty(actionReportText))
        {
            try
            {
                ActionReportLayout? deserializeActionReport = JsonConvert.DeserializeObject<ActionReportLayout>(actionReportText);
                if (deserializeActionReport != null)
                {
                    actionReport = deserializeActionReport;
                }
            }
            catch
            {
            }
        }
        return actionReport;
    }

    public YouRataConfiguration GetYouRataConfiguration()
    {
        YouRataConfiguration appConfig = new YouRataConfiguration();
        ActionIntelligenceServiceClient actionIntelligenceServiceClient = new ActionIntelligenceServiceClient(_conflictMonitorChannel);
        string configJson = actionIntelligenceServiceClient.GetActionIntelligence(new Empty()).ConfigJson;
        using (MemoryStream jsonMemoryStream = new MemoryStream(Encoding.ASCII.GetBytes(configJson)))
        {
            IConfigurationBuilder configurationBuilder = new ConfigurationBuilder()
                .AddJsonStream(jsonMemoryStream);
            configurationBuilder.Build().Bind(appConfig);
        }
        return appConfig;
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
