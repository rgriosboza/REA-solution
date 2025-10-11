using REA.Models.DTOs;
using REA.Models.Entities;

namespace REA.API.Services
{
    public interface IStudentService
    {
        Task<StudentResponse> CreateStudentAsync(CreateStudentRequest request);
        Task<StudentResponse?> GetStudentByIdAsync(int id);
        Task<List<StudentResponse>> GetAllStudentsAsync();
        Task<StudentResponse?> UpdateStudentAsync(int id, UpdateStudentRequest request);
        Task<bool> DeleteStudentAsync(int id);
        Task<List<StudentResponse>> GetStudentsByGradeAsync(string grade);
    }
}