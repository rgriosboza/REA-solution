using System.ComponentModel.DataAnnotations;

namespace REA.Models.DTOs
{
    public class CreatePaymentRequest
    {
        [Required]
        public int StudentId { get; set; }

        [Required]
        [Range(0.01, 100000)]
        public decimal Amount { get; set; }

        [Required]
        [StringLength(50)]
        public string PaymentType { get; set; } = string.Empty;

        [Required]
        public DateTime PaymentDate { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        public DateTime DueDate { get; set; }
    }

    public class PaymentResponse
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string PaymentType { get; set; } = string.Empty;
        public DateTime PaymentDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime DueDate { get; set; }
    }

    public class UpdatePaymentRequest
    {
        public decimal? Amount { get; set; }
        public string? PaymentType { get; set; }
        public string? Status { get; set; }
        public string? Description { get; set; }
        public DateTime? DueDate { get; set; }
    }
}