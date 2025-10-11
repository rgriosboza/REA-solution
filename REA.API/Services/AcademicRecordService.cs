using Microsoft.EntityFrameworkCore;
using REA.Models.Data;
using REA.Models.DTOs;
using REA.Models.Entities;

namespace REA.API.Services
{
    public class AcademicRecordService : IAcademicRecordService
    {
        private readonly ApplicationDbContext _context;

        public AcademicRecordService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<AcademicRecordResponse> CreateAcademicRecordAsync(CreateAcademicRecordRequest request)
        {
            var academicRecord = new AcademicRecord
            {
                StudentId = request.StudentId,
                TeacherId = request.TeacherId,
                Subject = request.Subject,
                Grade = request.Grade,
                Term = request.Term,
                SchoolYear = request.SchoolYear,
                Comments = request.Comments,
                RecordDate = DateTime.UtcNow
            };

            _context.AcademicRecords.Add(academicRecord);
            await _context.SaveChangesAsync();

            return await MapToAcademicRecordResponse(academicRecord);
        }

        public async Task<AcademicRecordResponse?> GetAcademicRecordByIdAsync(int id)
        {
            var academicRecord = await _context.AcademicRecords
                .Include(ar => ar.Student)
                .Include(ar => ar.Teacher)
                .FirstOrDefaultAsync(ar => ar.Id == id);

            return academicRecord != null ? await MapToAcademicRecordResponse(academicRecord) : null;
        }

        public async Task<List<AcademicRecordResponse>> GetAcademicRecordsByStudentAsync(int studentId)
        {
            var academicRecords = await _context.AcademicRecords
                .Include(ar => ar.Student)
                .Include(ar => ar.Teacher)
                .Where(ar => ar.StudentId == studentId)
                .OrderByDescending(ar => ar.RecordDate)
                .ToListAsync();

            var responses = new List<AcademicRecordResponse>();
            foreach (var record in academicRecords)
            {
                responses.Add(await MapToAcademicRecordResponse(record));
            }
            return responses;
        }

        public async Task<List<AcademicRecordResponse>> GetAcademicRecordsByTeacherAsync(int teacherId)
        {
            var academicRecords = await _context.AcademicRecords
                .Include(ar => ar.Student)
                .Include(ar => ar.Teacher)
                .Where(ar => ar.TeacherId == teacherId)
                .OrderByDescending(ar => ar.RecordDate)
                .ToListAsync();

            var responses = new List<AcademicRecordResponse>();
            foreach (var record in academicRecords)
            {
                responses.Add(await MapToAcademicRecordResponse(record));
            }
            return responses;
        }

        public async Task<AcademicRecordResponse?> UpdateAcademicRecordAsync(int id, CreateAcademicRecordRequest request)
        {
            var academicRecord = await _context.AcademicRecords.FindAsync(id);
            if (academicRecord == null) return null;

            academicRecord.StudentId = request.StudentId;
            academicRecord.TeacherId = request.TeacherId;
            academicRecord.Subject = request.Subject;
            academicRecord.Grade = request.Grade;
            academicRecord.Term = request.Term;
            academicRecord.SchoolYear = request.SchoolYear;
            academicRecord.Comments = request.Comments;

            await _context.SaveChangesAsync();

            return await MapToAcademicRecordResponse(academicRecord);
        }

        public async Task<bool> DeleteAcademicRecordAsync(int id)
        {
            var academicRecord = await _context.AcademicRecords.FindAsync(id);
            if (academicRecord == null) return false;

            _context.AcademicRecords.Remove(academicRecord);
            await _context.SaveChangesAsync();

            return true;
        }

        private async Task<AcademicRecordResponse> MapToAcademicRecordResponse(AcademicRecord academicRecord)
        {
            // Load related entities if not already loaded
            var student = academicRecord.Student ?? await _context.Students.FindAsync(academicRecord.StudentId);
            var teacher = academicRecord.Teacher ?? await _context.Teachers.FindAsync(academicRecord.TeacherId);

            return new AcademicRecordResponse
            {
                Id = academicRecord.Id,
                StudentId = academicRecord.StudentId,
                StudentName = student != null ? $"{student.FirstName} {student.LastName}" : "Unknown Student",
                TeacherId = academicRecord.TeacherId,
                TeacherName = teacher != null ? $"{teacher.FirstName} {teacher.LastName}" : "Unknown Teacher",
                Subject = academicRecord.Subject,
                Grade = academicRecord.Grade,
                Term = academicRecord.Term,
                SchoolYear = academicRecord.SchoolYear,
                RecordDate = academicRecord.RecordDate,
                Comments = academicRecord.Comments
            };
        }
    }
}