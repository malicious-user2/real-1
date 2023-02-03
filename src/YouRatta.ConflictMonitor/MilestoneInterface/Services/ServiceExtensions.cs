using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using YouRatta.Common.Configurations;
using YouRatta.Common.GitHub;

namespace YouRatta.ConflictMonitor.MilestoneInterface.Services;

public static class ServiceExtensions
{
    public static void AddAppConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddTransient<IValidator, ConfigurationsValidator>();
        services.AddOptions();

        services.Configure<YouRattaConfiguration>(configuration);

        services.AddSingleton<IValidatableConfiguration>(resolver => resolver.GetRequiredService<IOptions<YouRattaConfiguration>>().Value);
    }

    public static void AddConfigurationWriter(this IServiceCollection services, string settingsFilePath)
    {
        ConfigurationWriter writer = new ConfigurationWriter(settingsFilePath);
        services.TryAddSingleton<IConfigurationWriter>(writer);
    }

    public static void AddGitHubEnvironment(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<GitHubEnvironment>(configuration);
    }
}
