using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace YouRatta.Common.Configurations;

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
    [RegularExpression(@"^(top|bottom)$", ErrorMessage = "ErrataBulletinConfiguration.VideoTitleLocation must be top or bottom")]
    public string VideoTitleLocation { get; set; }

    [Required]
    [DefaultValue("Enter your eratta here")]
    public string Instructions { get; set; }
}
