// Copyright (c) 2023 battleship-systems.
// Licensed under the MIT license.

using System;
using Microsoft.Extensions.Configuration;

namespace YouRata.Common.GitHub;

public sealed class GitHubEnvironmentBuilder
{
    public IConfiguration Build()
    {
        return new ConfigurationBuilder()
            .AddEnvironmentVariables("GITHUB_")
            .Build();
    }
}
