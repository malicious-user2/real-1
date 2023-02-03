using System;
using System.ComponentModel.DataAnnotations;

namespace YouRatta.Common.Configurations;

public class YouRattaConfiguration : BaseValidatableConfiguration
{
    [Required]
    public bool ActionEnabled { get; set; }
}
