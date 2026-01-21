using System.ComponentModel.DataAnnotations;

namespace ResturangFrontEnd.Services;

public sealed class EmailOptions
{
    public const string SectionName = "Email";

    [Required]
    public string SmtpHost { get; set; } = "";

    [Range(1, 65535)]
    public int SmtpPort { get; set; } = 587;

    [Required]
    public string SmtpUser { get; set; } = "";

    [Required]
    public string SmtpPass { get; set; } = "";

    [Required]
    public string FromAddress { get; set; } = "";

    public string FromName { get; set; } = "";
}
