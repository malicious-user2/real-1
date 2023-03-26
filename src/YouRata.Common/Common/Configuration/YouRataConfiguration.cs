// Copyright (c) 2023 battleship-systems.
// Licensed under the MIT license.

using System;
using System.ComponentModel.DataAnnotations;
using YouRata.Common.Configuration.ActionCutOuts;
using YouRata.Common.Configuration.ErrataBulletin;
using YouRata.Common.Configuration.MilestoneLifetime;
using YouRata.Common.Configuration.YouTube;

namespace YouRata.Common.Configuration;

public class YouRataConfiguration : BaseValidatableConfiguration
{
    public YouRataConfiguration()
    {
        ActionCutOuts = new ActionCutOutConfiguration();
        MilestoneLifetime = new MilestoneLifetimeConfiguration();
        ErrataBulletin = new ErrataBulletinConfiguration();
        YouTube = new YouTubeConfiguration();
    }

    [Required] public ActionCutOutConfiguration ActionCutOuts { get; set; }

    [Required] public ErrataBulletinConfiguration ErrataBulletin { get; set; }

    [Required] public MilestoneLifetimeConfiguration MilestoneLifetime { get; set; }

    [Required] public YouTubeConfiguration YouTube { get; set; }

    public void ValidateConfigurationMembers()
    {
        ActionCutOuts.Validate();
        MilestoneLifetime.Validate();
        ErrataBulletin.Validate();
        YouTube.Validate();
    }
}
