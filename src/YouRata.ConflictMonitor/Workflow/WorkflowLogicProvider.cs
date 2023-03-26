// Copyright (c) 2023 battleship-systems.
// Licensed under the MIT license.

using System;
using YouRata.Common.YouTube;

namespace YouRata.ConflictMonitor.Workflow;

internal static class WorkflowLogicProvider
{
    internal static void ProcessWorkflow(ConflictMonitorWorkflow workflow)
    {
        if (YouTubeAPIHelper.GetTokenResponse(workflow.StoredTokenResponse, out _))
        {
            workflow.InitialSetupComplete = true;
        }
    }
}
