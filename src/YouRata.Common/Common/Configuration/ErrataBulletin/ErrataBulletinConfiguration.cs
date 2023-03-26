// Copyright (c) 2023 battleship-systems.
// Licensed under the MIT license.

using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace YouRata.Common.Configuration.ErrataBulletin;

public class ErrataBulletinConfiguration : BaseValidatableConfiguration
{
#pragma warning disable CS8618
    [Required]
    [Range(1, 50)]
    [DefaultValue(3)]
    public int EmptyLinesPerMark { get; set; }

    [Required]
    [DefaultValue("Enter your errata here")]
    public string Instructions { get; set; }

    [Required]
    [Range(1, 50)]
    [DefaultValue(30)]
    public int SecondsPerMark { get; set; }

    [Required]
    [DefaultValue("top")]
    [RegularExpression(@"^(top|bottom)$", ErrorMessage = "ErrataBulletinConfiguration.TitleLocation must be top or bottom")]
    public string TitleLocation { get; set; }
#pragma warning restore CS8618
}
