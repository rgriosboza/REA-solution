namespace REA.Models.DTOs
{
    public class CreateAcademicRecordRequest
    {
        public int StudentId { get; set; }
        public int TeacherId { get; set; }
        public string Subject { get; set; } = string.Empty;
        public decimal Grade { get; set; }
        public string Term { get; set; } = string.Empty;
        public int SchoolYear { get; set; }
        public string? Comments { get; set; }
    }

    public class AcademicRecordResponse
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public int TeacherId { get; set; }
        public string TeacherName { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public decimal Grade { get; set; }
        public string Term { get; set; } = string.Empty;
        public int SchoolYear { get; set; }
        public DateTime RecordDate { get; set; }
        public string? Comments { get; set; }
    }
}