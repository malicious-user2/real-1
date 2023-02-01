using System;
using System.ComponentModel.DataAnnotations;

namespace YouRatta.Common.Configurations;

public class ActionCutOutConfiguration : BaseValidatableConfiguration
{
    [Required]
    public bool ActionEnabled;
}
