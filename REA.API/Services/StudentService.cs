using Microsoft.EntityFrameworkCore;
using REA.Models.Data;
using REA.Models.DTOs;
using REA.Models.Entities;

namespace REA.API.Services
{
    public class StudentService : IStudentService
    {
        private readonly ApplicationDbContext _context;

        public StudentService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<StudentResponse> CreateStudentAsync(CreateStudentRequest request)
        {
            var student = new Student
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                PhoneNumber = request.PhoneNumber,
                DateOfBirth = request.DateOfBirth,
                Grade = request.Grade,
                Section = request.Section,
                EnrollmentDate = DateTime.UtcNow,
                IsActive = true
            };

            _context.Students.Add(student);
            await _context.SaveChangesAsync();

            return MapToStudentResponse(student);
        }

        public async Task<StudentResponse?> GetStudentByIdAsync(int id)
        {
            var student = await _context.Students
                .FirstOrDefaultAsync(s => s.Id == id);

            return student != null ? MapToStudentResponse(student) : null;
        }

        public async Task<List<StudentResponse>> GetAllStudentsAsync()
        {
            var students = await _context.Students
                .Where(s => s.IsActive)
                .OrderBy(s => s.LastName)
                .ThenBy(s => s.FirstName)
                .ToListAsync();

            return students.Select(MapToStudentResponse).ToList();
        }

        public async Task<StudentResponse?> UpdateStudentAsync(int id, UpdateStudentRequest request)
        {
            var student = await _context.Students.FindAsync(id);
            if (student == null) return null;

            if (!string.IsNullOrEmpty(request.FirstName))
                student.FirstName = request.FirstName;

            if (!string.IsNullOrEmpty(request.LastName))
                student.LastName = request.LastName;

            if (!string.IsNullOrEmpty(request.Email))
                student.Email = request.Email;

            if (!string.IsNullOrEmpty(request.PhoneNumber))
                student.PhoneNumber = request.PhoneNumber;

            if (!string.IsNullOrEmpty(request.Grade))
                student.Grade = request.Grade;

            if (!string.IsNullOrEmpty(request.Section))
                student.Section = request.Section;

            if (request.IsActive.HasValue)
                student.IsActive = request.IsActive.Value;

            await _context.SaveChangesAsync();

            return MapToStudentResponse(student);
        }

        public async Task<bool> DeleteStudentAsync(int id)
        {
            var student = await _context.Students.FindAsync(id);
            if (student == null) return false;

            student.IsActive = false;
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<List<StudentResponse>> GetStudentsByGradeAsync(string grade)
        {
            var students = await _context.Students
                .Where(s => s.Grade == grade && s.IsActive)
                .OrderBy(s => s.LastName)
                .ThenBy(s => s.FirstName)
                .ToListAsync();

            return students.Select(MapToStudentResponse).ToList();
        }

        private static StudentResponse MapToStudentResponse(Student student)
        {
            return new StudentResponse
            {
                Id = student.Id,
                FirstName = student.FirstName,
                LastName = student.LastName,
                Email = student.Email,
                PhoneNumber = student.PhoneNumber,
                DateOfBirth = student.DateOfBirth,
                Grade = student.Grade,
                Section = student.Section,
                EnrollmentDate = student.EnrollmentDate,
                IsActive = student.IsActive
            };
        }
    }
}