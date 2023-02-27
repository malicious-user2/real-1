using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace YouRatta.Common.Configurations;

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
    [DefaultValue("To see eratta for this video: {0}")]
    public string ErrataLinkTemplate { get; set; }
}
