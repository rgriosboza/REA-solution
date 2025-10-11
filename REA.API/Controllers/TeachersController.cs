using Microsoft.AspNetCore.Mvc;
using REA.API.Services;
using REA.Models.DTOs;

namespace REA.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TeachersController : ControllerBase
    {
        private readonly ITeacherService _teacherService;

        public TeachersController(ITeacherService teacherService)
        {
            _teacherService = teacherService;
        }

        [HttpGet]
        public async Task<ActionResult<List<TeacherResponse>>> GetAllTeachers()
        {
            var teachers = await _teacherService.GetAllTeachersAsync();
            return Ok(teachers);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TeacherResponse>> GetTeacherById(int id)
        {
            var teacher = await _teacherService.GetTeacherByIdAsync(id);
            if (teacher == null) return NotFound();
            return Ok(teacher);
        }

        [HttpPost]
        public async Task<ActionResult<TeacherResponse>> CreateTeacher(CreateTeacherRequest request)
        {
            var teacher = await _teacherService.CreateTeacherAsync(request);
            return CreatedAtAction(nameof(GetTeacherById), new { id = teacher.Id }, teacher);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<TeacherResponse>> UpdateTeacher(int id, UpdateTeacherRequest request)
        {
            var teacher = await _teacherService.UpdateTeacherAsync(id, request);
            if (teacher == null) return NotFound();
            return Ok(teacher);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteTeacher(int id)
        {
            var result = await _teacherService.DeleteTeacherAsync(id);
            if (!result) return NotFound();
            return NoContent();
        }
    }
}