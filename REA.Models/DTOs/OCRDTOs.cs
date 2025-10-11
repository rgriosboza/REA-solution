namespace REA.Models.DTOs
{
    public class OCRProcessRequest
    {
        public string ImageBase64 { get; set; } = string.Empty;
        public string DocumentType { get; set; } = string.Empty; // "GradeRecord", "Payment", "StudentInfo"
    }

    public class OCRProcessResponse
    {
        public bool Success { get; set; }
        public string ExtractedText { get; set; } = string.Empty;
        public ProcessedData? Data { get; set; }
        public string? Error { get; set; }
    }

    public class ProcessedData
    {
        public StudentResponse? Student { get; set; }
        public AcademicRecordResponse? AcademicRecord { get; set; }
        public PaymentResponse? Payment { get; set; }
    }
}