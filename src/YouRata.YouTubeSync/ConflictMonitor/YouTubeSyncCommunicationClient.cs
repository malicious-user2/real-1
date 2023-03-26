// Copyright (c) 2023 battleship-systems.
// Licensed under the MIT license.

using System;
using YouRata.Common.Milestone;
using static YouRata.Common.Proto.MilestoneActionIntelligence.Types;

namespace YouRata.YouTubeSync.ConflictMonitor;

internal class YouTubeSyncCommunicationClient : MilestoneCommunicationClient
{
    private static readonly string _milestoneName = "YouTubeSync";
    private readonly Type _milestoneType = typeof(YouTubeSyncActionIntelligence);

    public YouTubeSyncCommunicationClient() : base()
    {
    }

    public bool Activate(out YouTubeSyncActionIntelligence intelligence)
    {
        intelligence = new YouTubeSyncActionIntelligence();
        if (base.IsBlocked(_milestoneType)) return false;
        intelligence = base.Activate<YouTubeSyncActionIntelligence>(_milestoneType, _milestoneName);
        return true;
    }

    public YouTubeSyncActionIntelligence? GetMilestoneActionIntelligence()
    {
        YouTubeSyncActionIntelligence? youTubeSyncActionIntelligence = null;
        object? milestoneActionIntelligence = base.GetMilestoneActionIntelligence(_milestoneType);
        if (milestoneActionIntelligence != null)
        {
            youTubeSyncActionIntelligence = (YouTubeSyncActionIntelligence?)milestoneActionIntelligence;
        }

        return youTubeSyncActionIntelligence;
    }

    public void Keepalive()
    {
        base.Keepalive(_milestoneType, _milestoneName);
    }

    public void LogMessage(string message)
    {
        base.LogMessage(message, _milestoneName);
    }

    public void SetMilestoneActionIntelligence(YouTubeSyncActionIntelligence youTubeSyncActionIntelligence)
    {
        base.SetMilestoneActionIntelligence(youTubeSyncActionIntelligence, _milestoneType, _milestoneName);
    }

    public void SetStatus(MilestoneCondition status)
    {
        base.SetStatus(status, _milestoneType, _milestoneName);
    }
}
