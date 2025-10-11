using Microsoft.AspNetCore.Mvc;
using REA.API.Services;
using REA.Models.DTOs;

namespace REA.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StudentsController : ControllerBase
    {
        private readonly IStudentService _studentService;

        public StudentsController(IStudentService studentService)
        {
            _studentService = studentService;
        }

        [HttpGet]
        public async Task<ActionResult<List<StudentResponse>>> GetAllStudents()
        {
            var students = await _studentService.GetAllStudentsAsync();
            return Ok(students);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<StudentResponse>> GetStudentById(int id)
        {
            var student = await _studentService.GetStudentByIdAsync(id);
            if (student == null) return NotFound();
            return Ok(student);
        }

        [HttpPost]
        public async Task<ActionResult<StudentResponse>> CreateStudent(CreateStudentRequest request)
        {
            var student = await _studentService.CreateStudentAsync(request);
            return CreatedAtAction(nameof(GetStudentById), new { id = student.Id }, student);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<StudentResponse>> UpdateStudent(int id, UpdateStudentRequest request)
        {
            var student = await _studentService.UpdateStudentAsync(id, request);
            if (student == null) return NotFound();
            return Ok(student);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteStudent(int id)
        {
            var result = await _studentService.DeleteStudentAsync(id);
            if (!result) return NotFound();
            return NoContent();
        }

        [HttpGet("grade/{grade}")]
        public async Task<ActionResult<List<StudentResponse>>> GetStudentsByGrade(string grade)
        {
            var students = await _studentService.GetStudentsByGradeAsync(grade);
            return Ok(students);
        }
    }
}