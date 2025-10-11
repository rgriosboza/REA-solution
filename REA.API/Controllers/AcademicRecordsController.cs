using Microsoft.AspNetCore.Mvc;
using REA.API.Services;
using REA.Models.DTOs;

namespace REA.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AcademicRecordsController : ControllerBase
    {
        private readonly IAcademicRecordService _academicRecordService;

        public AcademicRecordsController(IAcademicRecordService academicRecordService)
        {
            _academicRecordService = academicRecordService;
        }

        [HttpGet]
        public async Task<ActionResult<List<AcademicRecordResponse>>> GetAllAcademicRecords()
        {
            var records = await _academicRecordService.GetAcademicRecordsByStudentAsync(1); // Temporal
            return Ok(records);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<AcademicRecordResponse>> GetAcademicRecordById(int id)
        {
            var record = await _academicRecordService.GetAcademicRecordByIdAsync(id);
            if (record == null) return NotFound();
            return Ok(record);
        }

        [HttpPost]
        public async Task<ActionResult<AcademicRecordResponse>> CreateAcademicRecord(CreateAcademicRecordRequest request)
        {
            var record = await _academicRecordService.CreateAcademicRecordAsync(request);
            return CreatedAtAction(nameof(GetAcademicRecordById), new { id = record.Id }, record);
        }

        [HttpGet("student/{studentId}")]
        public async Task<ActionResult<List<AcademicRecordResponse>>> GetAcademicRecordsByStudent(int studentId)
        {
            var records = await _academicRecordService.GetAcademicRecordsByStudentAsync(studentId);
            return Ok(records);
        }

        [HttpGet("teacher/{teacherId}")]
        public async Task<ActionResult<List<AcademicRecordResponse>>> GetAcademicRecordsByTeacher(int teacherId)
        {
            var records = await _academicRecordService.GetAcademicRecordsByTeacherAsync(teacherId);
            return Ok(records);
        }
    }
}