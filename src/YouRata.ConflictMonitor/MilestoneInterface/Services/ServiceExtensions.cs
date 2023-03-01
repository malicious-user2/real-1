using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using YouRata.Common.Configurations;
using YouRata.Common.GitHub;

namespace YouRata.ConflictMonitor.MilestoneInterface.Services;

public static class ServiceExtensions
{
    public static void AddAppConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddTransient<IValidator, ConfigurationsValidator>();
        services.AddOptions();

        services.Configure<YouRataConfiguration>(configuration);

        services.AddSingleton<IValidatableConfiguration>(resolver => resolver.GetRequiredService<IOptions<YouRataConfiguration>>().Value);
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