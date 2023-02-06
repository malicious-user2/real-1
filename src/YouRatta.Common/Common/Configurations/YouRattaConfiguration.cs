using System;
using System.ComponentModel.DataAnnotations;

namespace YouRatta.Common.Configurations;

public class YouRattaConfiguration : BaseValidatableConfiguration
{
    public YouRattaConfiguration()
    {
        ActionCutOuts = new ActionCutOutConfiguration();
    }

    [Required]
    public ActionCutOutConfiguration ActionCutOuts { get; set; }
}
