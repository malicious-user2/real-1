using System;
using System.ComponentModel.DataAnnotations;

namespace YouRata.Common.Configurations;

public class MilestoneLifetimeConfiguration : BaseValidatableConfiguration
{
    [Required]
    [Range(5, 9000)]
    public int MaxUpdateDwellTime { get; set; }

    [Required]
    [Range(5, 9000)]
    public int MaxRunTime { get; set; }
}
