using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace YouRata.Common.Configurations;

public class ErrataBulletinConfiguration : BaseValidatableConfiguration
{
    [Required]
    [Range(1, 50)]
    public int EmptyLinesPerMark { get; set; }

    [Required]
    [Range(1, 50)]
    public int SecondsPerMark { get; set; }

    [Required]
    [DefaultValue("top")]
    [RegularExpression(@"^(top|bottom)$", ErrorMessage = "ErrataBulletinConfiguration.TitleLocation must be top or bottom")]
    public string TitleLocation { get; set; }

    [Required]
    [DefaultValue("Enter your errata here")]
    public string Instructions { get; set; }
}
