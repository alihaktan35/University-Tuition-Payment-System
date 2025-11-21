using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace APIGateway.Models;

public class RateLimit
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int RateLimitId { get; set; }

    [Required]
    [StringLength(10)]
    public string StudentNo { get; set; } = string.Empty;

    [Required]
    [StringLength(200)]
    public string Endpoint { get; set; } = string.Empty;

    [Required]
    public int CallCount { get; set; } = 0;

    [Required]
    public DateTime Date { get; set; } = DateTime.UtcNow.Date;

    public DateTime LastCall { get; set; } = DateTime.UtcNow;
}
