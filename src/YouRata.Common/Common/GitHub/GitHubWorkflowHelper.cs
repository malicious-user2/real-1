// Copyright (c) 2023 battleship-systems.
// Licensed under the MIT license.

using System;
using System.IO;

namespace YouRata.Common.GitHub;

public static class GitHubWorkflowHelper
{
    private const string GitHubEnvironmentFile = "GITHUB_ENV";
    private const string GitHubStepSummaryFile = "GITHUB_STEP_SUMMARY";

    public static void PushVariable(string variable, string value)
    {
        string? environmentFile = Environment.GetEnvironmentVariable(GitHubEnvironmentFile);
        if (environmentFile == null) return;
        using (StreamWriter variableWriter = File.AppendText(environmentFile))
        {
            variableWriter.WriteLine($"{variable}={value}");
        }
    }

    public static void WriteStepSummary(string markdown)
    {
        string? summaryFile = Environment.GetEnvironmentVariable(GitHubStepSummaryFile);
        if (summaryFile == null) return;
        using (StreamWriter summaryWriter = File.AppendText(summaryFile))
        {
            summaryWriter.Write(markdown);
        }
    }
}
