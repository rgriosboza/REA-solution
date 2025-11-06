namespace REA.Models.DTOs
{
    public class OCRProcessRequest
    {
        public string ImageBase64 { get; set; } = string.Empty;
        public string DocumentType { get; set; } = "AcademicRecord"; // AcademicRecord, IDCard, etc.
    }

    public class OCRProcessResponse
    {
        public bool Success { get; set; }
        public string? ExtractedText { get; set; }
        public ProcessedData? Data { get; set; }
        public string? Error { get; set; }
    }

    public class ProcessedData
    {
        public StudentData? Student { get; set; }
        public AcademicData? Academic { get; set; }
        public Dictionary<string, string>? AdditionalFields { get; set; }
    }

    public class StudentData
    {
        public string? FullName { get; set; }
        public string? StudentId { get; set; }
        public string? Grade { get; set; }
        public string? Section { get; set; }
    }

    public class AcademicData
    {
        public string? Subject { get; set; }
        public decimal? Score { get; set; }
        public string? Period { get; set; }
        public string? SchoolYear { get; set; }
    }
}