using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace REA.Models.Entities
{
    public class AcademicRecord
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int StudentId { get; set; }

        [Required]
        public int TeacherId { get; set; }

        [Required]
        [StringLength(100)]
        public string Subject { get; set; } = string.Empty;

        // CAMBIO: Calificación final del período (0-100)
        [Required]
        [Column(TypeName = "decimal(5,2)")]
        [Range(0, 100)]
        public decimal FinalGrade { get; set; }

        // CAMBIO: Período específico (1-4)
        [Required]
        [Range(1, 4)]
        public int Period { get; set; }

        [Required]
        public int SchoolYear { get; set; }

        public DateTime RecordDate { get; set; } = DateTime.UtcNow;

        [StringLength(500)]
        public string? Comments { get; set; }

        // Navigation properties
        [ForeignKey("StudentId")]
        public virtual Student Student { get; set; } = null!;

        [ForeignKey("TeacherId")]
        public virtual Teacher Teacher { get; set; } = null!;
    }
}