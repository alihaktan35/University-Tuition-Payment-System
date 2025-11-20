using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TuitionPaymentAPI.Models;

public class Payment
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int PaymentId { get; set; }

    [Required]
    public int TuitionId { get; set; }

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Amount { get; set; }

    [Required]
    public DateTime PaymentDate { get; set; } = DateTime.UtcNow;

    [Required]
    [StringLength(50)]
    public string Status { get; set; } = "Successful"; // Successful, Error

    [StringLength(100)]
    public string TransactionReference { get; set; } = string.Empty;

    // Navigation property
    [ForeignKey("TuitionId")]
    public virtual Tuition Tuition { get; set; } = null!;
}
