using REA.Models.DTOs;

namespace REA.API.Services
{
    public interface IOCRService
    {
        Task<OCRProcessResponse> ProcessImageAsync(OCRProcessRequest request);
        Task<string> ExtractTextFromImageAsync(string imageBase64);
    }
}