using System;
using System.ComponentModel.DataAnnotations;

namespace YouRatta.Common.Configurations;

public class ActionCutOutConfiguration : BaseValidatableConfiguration
{
    [Required]
    public bool DisableConflictMonitorGitHubOperations { get; set; }

    [Required]
    public bool DisableStoredTokenResponseDiscovery { get; set; }

    [Required]
    public bool DisableInitialSetupMilestone { get; set; }

    [Required]
    public bool DisableUnsupportedGitHubAPI { get; set; }

    [Required]
    public bool DisableYouTubeSyncMilestone { get; set; }

    [Required]
    public bool DisableYouTubeVideoUpdate { get; set; }
}
