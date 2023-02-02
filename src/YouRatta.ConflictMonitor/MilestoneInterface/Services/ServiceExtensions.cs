using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using YouRatta.Common.Configurations;

namespace YouRatta.ConflictMonitor.MilestoneInterface.Services;

public static class ServiceExtensions
{
    public static void AddAppConfigurations(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions();

        services.Configure<ActionCutOutConfiguration>(configuration.GetSection("PrometheusExporterConfiguration"));
        services.Configure<LivenessConfiguration>(configuration.GetSection("LivenessConfiguration"));
    }
}
