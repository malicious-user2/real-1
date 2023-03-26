// Copyright (c) 2023 battleship-systems.
// Licensed under the MIT license.

using System;
using Octokit;

namespace YouRata.Common.GitHub;

public static class GitHubConstants
{
    public const string ProductHeaderName = "YouRata";

    public static readonly TimeSpan RequestTimeout = TimeSpan.FromMilliseconds(1000 * 30);

    public static ProductHeaderValue ProductHeader => new(ProductHeaderName);
}
