using System.ComponentModel.DataAnnotations;

namespace REA.Models.Entities
{
    public class Student
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string LastName { get; set; } = string.Empty;

        [StringLength(100)]
        public string? Email { get; set; }

        [StringLength(20)]
        public string? PhoneNumber { get; set; }

        [Required]
        public DateTime DateOfBirth { get; set; }

        [Required]
        [StringLength(10)]
        public string Grade { get; set; } = string.Empty; // e.g., "1st", "2nd"

        [StringLength(50)]
        public string? Section { get; set; } // e.g., "A", "B"

        public DateTime EnrollmentDate { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;

        // Navigation properties
        public virtual ICollection<AcademicRecord> AcademicRecords { get; set; } = new List<AcademicRecord>();
        public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
    }
}