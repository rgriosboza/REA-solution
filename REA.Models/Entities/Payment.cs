using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace REA.Models.Entities
{
    public class Payment
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int StudentId { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal Amount { get; set; }

        [Required]
        [StringLength(50)]
        public string PaymentType { get; set; } = string.Empty; // "Tuition", "Books", "Uniform", etc.

        [Required]
        public DateTime PaymentDate { get; set; }

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Pending"; // "Pending", "Completed", "Cancelled"

        [StringLength(500)]
        public string? Description { get; set; }

        public DateTime DueDate { get; set; }

        // Navigation properties
        [ForeignKey("StudentId")]
        public virtual Student Student { get; set; } = null!;
    }
}