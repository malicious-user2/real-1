using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace YouRatta.ConflictMonitor.MilestoneInterface.Services;

public static class ServiceExtensions
{
    public static void AddAppConfigurations(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions();

        services.Configure<PrometheusExporterConfiguration>(configuration.GetSection("PrometheusExporterConfiguration"));
        services.Configure<LivenessConfiguration>(configuration.GetSection("LivenessConfiguration"));
    }
}
