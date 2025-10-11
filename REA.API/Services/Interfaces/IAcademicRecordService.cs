using REA.Models.DTOs;

namespace REA.API.Services
{
    public interface IAcademicRecordService
    {
        Task<AcademicRecordResponse> CreateAcademicRecordAsync(CreateAcademicRecordRequest request);
        Task<AcademicRecordResponse?> GetAcademicRecordByIdAsync(int id);
        Task<List<AcademicRecordResponse>> GetAcademicRecordsByStudentAsync(int studentId);
        Task<List<AcademicRecordResponse>> GetAcademicRecordsByTeacherAsync(int teacherId);
        Task<AcademicRecordResponse?> UpdateAcademicRecordAsync(int id, CreateAcademicRecordRequest request);
        Task<bool> DeleteAcademicRecordAsync(int id);
    }
}