using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace YouRatta.Common.Configurations;

public class ErrataBulletinConfiguration : BaseValidatableConfiguration
{

    [Required]
    [Range(0, 50)]
    public int EmptyLinesPerTimeInterval { get; set; }

    [Required]
    [DefaultValue("top")]
    [RegularExpression(@"^(top|bottom)$", ErrorMessage = "VideoTitleLocation must be top or bottom")]
    public string VideoTitleLocation { get; set; }
}
