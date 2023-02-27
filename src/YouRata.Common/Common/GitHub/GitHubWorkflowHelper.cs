using System;
using System.IO;

namespace YouRata.Common.GitHub;

public static class GitHubWorkflowHelper
{
    private const string GitHubEnvironmentFile = "GITHUB_ENV";

    public static void PushVariable(string variable, string value)
    {
        string? environmentFile = Environment.GetEnvironmentVariable(GitHubEnvironmentFile);
        if (environmentFile != null)
        {
            using (StreamWriter variableWriter = File.AppendText(environmentFile))
            {
                variableWriter.WriteLine($"{variable}={value}");
            }
        }
    }
}
