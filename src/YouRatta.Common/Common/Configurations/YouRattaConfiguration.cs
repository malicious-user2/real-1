using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Components.Web;

namespace YouRatta.Common.Configurations;

public class YouRattaConfiguration : BaseValidatableConfiguration
{
    public YouRattaConfiguration()
    {
        ActionCutOuts = new ActionCutOutConfiguration();
        MilestoneLifetime = new MilestoneLifetimeConfiguration();
        ErrataBulletin = new ErrataBulletinConfiguration();
        YouTube = new YouTubeConfiguration();
    }

    public void ValidateConfigurationMembers()
    {
        ActionCutOuts.Validate();
        MilestoneLifetime.Validate();
        ErrataBulletin.Validate();
        YouTube.Validate();
    }

    [Required]
    public YouTubeConfiguration YouTube { get; set; }

    [Required]
    public ActionCutOutConfiguration ActionCutOuts { get; set; }

    [Required]
    public MilestoneLifetimeConfiguration MilestoneLifetime { get; set; }

    [Required]
    public ErrataBulletinConfiguration ErrataBulletin { get; set; }
}
