// Copyright (c) 2023 battleship-systems.
// Licensed under the MIT license.

using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace YouRata.Common.Configuration.YouTube;

public class YouTubeConfiguration : BaseValidatableConfiguration
{
#pragma warning disable CS8618
    [RegularExpression(@"^UC[-_0-9A-Za-z]{21}[AQgw]$", ErrorMessage = "Invalid YouTubeConfiguration.ChannelId")]
    public string ChannelId { get; set; }

    [Required]
    [DefaultValue("bottom")]
    [RegularExpression(@"^(top|bottom)$", ErrorMessage = "YouTubeConfiguration.ErattaLinkLocation must be top or bottom")]
    public string ErattaLinkLocation { get; set; }

    [Required]
    [DefaultValue("To see errata for this video: {0}")]
    public string ErrataLinkTemplate { get; set; }

    public string[]? ExcludePlaylists { get; set; }

    public string[]? ExcludeVideos { get; set; }

    [Required]
    [DefaultValue(60)]
    public int MinVideoSeconds { get; set; }

    [Required]
    [DefaultValue(10000)]
    public int QueriesPerDay { get; set; }

    [Required]
    [DefaultValue(true)]
    public bool TruncateDescriptionOverflow { get; set; }

    [Required]
    [DefaultValue("any")]
    [RegularExpression(@"^(any|long|medium|short)$",
        ErrorMessage = "YouTubeConfiguration.VideoDurationFilter must be any, long, medium, or short")]
    public string VideoDurationFilter { get; set; }

#pragma warning restore CS8618
}
