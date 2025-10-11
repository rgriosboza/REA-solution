using Microsoft.EntityFrameworkCore;
using REA.Models.Data;
using REA.Models.DTOs;
using REA.Models.Entities;

namespace REA.API.Services
{
    public class TeacherService : ITeacherService
    {
        private readonly ApplicationDbContext _context;

        public TeacherService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<TeacherResponse> CreateTeacherAsync(CreateTeacherRequest request)
        {
            var teacher = new Teacher
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                PhoneNumber = request.PhoneNumber,
                Subject = request.Subject,
                HireDate = DateTime.UtcNow,
                IsActive = true
            };

            _context.Teachers.Add(teacher);
            await _context.SaveChangesAsync();

            return MapToTeacherResponse(teacher);
        }

        public async Task<TeacherResponse?> GetTeacherByIdAsync(int id)
        {
            var teacher = await _context.Teachers
                .FirstOrDefaultAsync(t => t.Id == id);

            return teacher != null ? MapToTeacherResponse(teacher) : null;
        }

        public async Task<List<TeacherResponse>> GetAllTeachersAsync()
        {
            var teachers = await _context.Teachers
                .Where(t => t.IsActive)
                .OrderBy(t => t.LastName)
                .ThenBy(t => t.FirstName)
                .ToListAsync();

            return teachers.Select(MapToTeacherResponse).ToList();
        }

        public async Task<TeacherResponse?> UpdateTeacherAsync(int id, UpdateTeacherRequest request)
        {
            var teacher = await _context.Teachers.FindAsync(id);
            if (teacher == null) return null;

            if (!string.IsNullOrEmpty(request.FirstName))
                teacher.FirstName = request.FirstName;

            if (!string.IsNullOrEmpty(request.LastName))
                teacher.LastName = request.LastName;

            if (!string.IsNullOrEmpty(request.Email))
                teacher.Email = request.Email;

            if (!string.IsNullOrEmpty(request.PhoneNumber))
                teacher.PhoneNumber = request.PhoneNumber;

            if (!string.IsNullOrEmpty(request.Subject))
                teacher.Subject = request.Subject;

            if (request.IsActive.HasValue)
                teacher.IsActive = request.IsActive.Value;

            await _context.SaveChangesAsync();

            return MapToTeacherResponse(teacher);
        }

        public async Task<bool> DeleteTeacherAsync(int id)
        {
            var teacher = await _context.Teachers.FindAsync(id);
            if (teacher == null) return false;

            teacher.IsActive = false;
            await _context.SaveChangesAsync();

            return true;
        }

        private static TeacherResponse MapToTeacherResponse(Teacher teacher)
        {
            return new TeacherResponse
            {
                Id = teacher.Id,
                FirstName = teacher.FirstName,
                LastName = teacher.LastName,
                Email = teacher.Email,
                PhoneNumber = teacher.PhoneNumber,
                Subject = teacher.Subject,
                HireDate = teacher.HireDate,
                IsActive = teacher.IsActive
            };
        }
    }
}