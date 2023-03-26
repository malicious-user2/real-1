// Copyright (c) 2023 battleship-systems.
// Licensed under the MIT license.

using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using YouRata.Common.Configuration;
using YouRata.Common.GitHub;

namespace YouRata.ConflictMonitor.MilestoneInterface.Services;

internal static class ServiceExtensions
{
    public static void AddAppConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddTransient<IValidator, ConfigurationsValidator>();
        services.AddOptions();

        services.Configure<YouRataConfiguration>(configuration);

        services.AddSingleton<IValidatableConfiguration>(resolver => resolver.GetRequiredService<IOptions<YouRataConfiguration>>().Value);
    }

    public static void AddGitHubEnvironment(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<GitHubEnvironment>(configuration);
    }
}
