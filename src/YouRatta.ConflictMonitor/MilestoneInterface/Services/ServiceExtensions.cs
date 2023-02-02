using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using YouRatta.Common.Configurations;

namespace YouRatta.ConflictMonitor.MilestoneInterface.Services;

public static class ServiceExtensions
{
    public static void AddAppConfigurations(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions();

        services.Configure<ActionCutOutConfiguration>(configuration.GetSection("ActionCutOutConfiguration"));

        services.AddSingleton<IValidatableConfiguration>(resolver => resolver.GetRequiredService<IOptions<ActionCutOutConfiguration>>().Value);
    }

    public static void AddConfigurationWriter(this IServiceCollection services)
    {
        var writer = new ConfigurationWriter(ConfigurationConstants.YouRattaSettingsPath);
        services.TryAddSingleton<IConfigurationWriter>(writer);
    }
}
