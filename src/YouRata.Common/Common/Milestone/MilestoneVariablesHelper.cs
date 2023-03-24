using System;
using YouRata.Common.Configuration;
using YouRata.Common.Proto;

namespace YouRata.Common.Milestone;

public static class MilestoneVariablesHelper
{
    public static void CreateRuntimeVariables(MilestoneCommunicationClient client, out ActionIntelligence actionInt, out YouRataConfiguration config, out GitHubActionEnvironment actionEnvironment)
    {
        actionInt = client.GetActionIntelligence();
        config = client.GetYouRataConfiguration();
        actionEnvironment = actionInt.GitHubActionEnvironment;
    }
}
