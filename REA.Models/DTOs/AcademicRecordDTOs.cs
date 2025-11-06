using System.ComponentModel.DataAnnotations;

namespace REA.Models.DTOs
{
    public class CreateAcademicRecordRequest
    {
        public int StudentId { get; set; }
        public int TeacherId { get; set; }
        public string Subject { get; set; } = string.Empty;
        
        // CAMBIO: Calificación final del período
        [Range(0, 100)]
        public decimal FinalGrade { get; set; }
        
        // CAMBIO: Período específico (1-4)
        [Range(1, 4)]
        public int Period { get; set; }
        
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
        
        // CAMBIO
        public decimal FinalGrade { get; set; }
        public int Period { get; set; }
        public string PeriodName => GetPeriodName(Period);
        
        public int SchoolYear { get; set; }
        public DateTime RecordDate { get; set; }
        public string? Comments { get; set; }

        private static string GetPeriodName(int period) => period switch
        {
            1 => "Primer Parcial",
            2 => "Segundo Parcial", 
            3 => "Tercer Parcial",
            4 => "Cuarto Parcial",
            _ => "Período Desconocido"
        };
    }

    public class UpdateAcademicRecordRequest
    {
        [Range(0, 100)]
        public decimal? FinalGrade { get; set; }
        
        [Range(1, 4)]
        public int? Period { get; set; }
        
        public int? SchoolYear { get; set; }
        public string? Comments { get; set; }
    }
}