using System.ComponentModel.DataAnnotations;

namespace YouRata.Common.Configuration.ActionCutOuts;

public class ActionCutOutConfiguration : BaseValidatableConfiguration
{
    [Required]
    public bool DisableConflictMonitorGitHubOperations { get; set; }

    [Required]
    public bool DisableInitialSetupMilestone { get; set; }

    [Required]
    public bool DisableUnsupportedGitHubAPI { get; set; }

    [Required]
    public bool DisableYouTubeSyncMilestone { get; set; }

    [Required]
    public bool DisableYouTubeVideoUpdate { get; set; }

    [Required]
    public bool DisableActionReportMilestone { get; set; }
}
