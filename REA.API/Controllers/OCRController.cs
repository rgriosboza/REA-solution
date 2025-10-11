using Microsoft.AspNetCore.Mvc;
using REA.API.Services;
using REA.Models.DTOs;

namespace REA.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OCRController : ControllerBase
    {
        private readonly IOCRService _ocrService;

        public OCRController(IOCRService ocrService)
        {
            _ocrService = ocrService;
        }

        [HttpPost("process")]
        public async Task<ActionResult<OCRProcessResponse>> ProcessDocument([FromBody] OCRProcessRequest request)
        {
            var result = await _ocrService.ProcessImageAsync(request);
            
            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }
    }
}