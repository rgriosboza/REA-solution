using REA.Models.DTOs;

namespace REA.API.Services
{
    public interface ITeacherService
    {
        Task<TeacherResponse> CreateTeacherAsync(CreateTeacherRequest request);
        Task<TeacherResponse?> GetTeacherByIdAsync(int id);
        Task<List<TeacherResponse>> GetAllTeachersAsync();
        Task<TeacherResponse?> UpdateTeacherAsync(int id, UpdateTeacherRequest request);
        Task<bool> DeleteTeacherAsync(int id);
    }
}