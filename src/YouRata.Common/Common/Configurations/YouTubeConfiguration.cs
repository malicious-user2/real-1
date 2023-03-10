using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace YouRata.Common.Configurations;

public class YouTubeConfiguration : BaseValidatableConfiguration
{
    [RegularExpression(@"^UC[-_0-9A-Za-z]{21}[AQgw]$", ErrorMessage = "Invalid YouTubeConfiguration.ChannelId")]
    public string ChannelId { get; set; }

    public string[] ExcludePlaylists { get; set; }

    [Required]
    [DefaultValue("bottom")]
    [RegularExpression(@"^(top|bottom)$", ErrorMessage = "YouTubeConfiguration.ErattaLinkLocation must be top or bottom")]
    public string ErattaLinkLocation { get; set; }

    [Required]
    [DefaultValue("To see errata for this video: {0}")]
    public string ErrataLinkTemplate { get; set; }

    [Required]
    [DefaultValue(10000)]
    public int QueriesPerDay { get; set; }

    [Required]
    [DefaultValue("any")]
    [RegularExpression(@"^(any|long|medium|short)$", ErrorMessage = "YouTubeConfiguration.VideoDurationFilter must be any, long, medium, or short")]
    public string VideoDurationFilter { get; set; }
}
