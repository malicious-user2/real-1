using System;
using System.ComponentModel.DataAnnotations;

namespace YouRatta.Common.Configurations;

public class YouRattaConfiguration : BaseValidatableConfiguration
{
    public YouRattaConfiguration()
    {
        ActionCutOuts = new ActionCutOutConfiguration();
        MilestoneLifetime = new MilestoneLifetimeConfiguration();
    }

    [Required]
    public ActionCutOutConfiguration ActionCutOuts { get; set; }

    [Required]
    public MilestoneLifetimeConfiguration MilestoneLifetime { get; set; }
}
