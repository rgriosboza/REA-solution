namespace REA.Models.DTOs
{
    public class CreateStudentRequest
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string Grade { get; set; } = string.Empty;
        public string? Section { get; set; }
    }

    public class StudentResponse
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string Grade { get; set; } = string.Empty;
        public string? Section { get; set; }
        public DateTime EnrollmentDate { get; set; }
        public bool IsActive { get; set; }
    }

    public class UpdateStudentRequest
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Grade { get; set; }
        public string? Section { get; set; }
        public bool? IsActive { get; set; }
    }
}