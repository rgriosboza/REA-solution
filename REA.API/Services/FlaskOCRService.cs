using System.Text.Json;
using System.Text.RegularExpressions;
using REA.Models.DTOs;

namespace REA.API.Services
{
    public class FlaskOCRService : IOCRService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<FlaskOCRService> _logger;

        public FlaskOCRService(
            IHttpClientFactory httpClientFactory,
            ILogger<FlaskOCRService> logger)
        {
            _httpClient = httpClientFactory.CreateClient("FlaskOCR");
            _logger = logger;
        }

        public async Task<OCRProcessResponse> ProcessImageAsync(OCRProcessRequest request)
        {
            try
            {
                _logger.LogInformation("Processing OCR request for document type: {DocumentType}", 
                    request.DocumentType);

                // Convert base64 to bytes and then to IFormFile-like structure
                var imageBytes = Convert.FromBase64String(request.ImageBase64);
                
                using var content = new MultipartFormDataContent();
                var imageContent = new ByteArrayContent(imageBytes);
                imageContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/png");
                content.Add(imageContent, "imagen", "document.png");

                // Send to Flask service
                var response = await _httpClient.PostAsync("/segmentar", content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Flask server error: {Status} - {Content}", 
                        response.StatusCode, errorContent);
                    
                    return new OCRProcessResponse
                    {
                        Success = false,
                        Error = $"Error del servidor OCR: {response.StatusCode}"
                    };
                }

                var jsonResponse = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("Flask response received: {Length} chars", jsonResponse.Length);

                var result = JsonSerializer.Deserialize<FlaskOCRResponse>(jsonResponse, 
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (result == null)
                {
                    return new OCRProcessResponse
                    {
                        Success = false,
                        Error = "No se pudo procesar la respuesta del servidor OCR"
                    };
                }

                // Parse results
                var processedData = ParseFlaskResults(result.NombresDetectados, request.DocumentType);

                return new OCRProcessResponse
                {
                    Success = true,
                    ExtractedText = string.Join("\n", result.NombresDetectados ?? new List<string>()),
                    Data = processedData
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing OCR request");
                return new OCRProcessResponse
                {
                    Success = false,
                    Error = $"Error al procesar el documento: {ex.Message}"
                };
            }
        }

        public async Task<string> ExtractTextFromImageAsync(string imageBase64)
        {
            try
            {
                _logger.LogInformation("Extracting text from image");

                // Convert base64 to bytes
                var imageBytes = Convert.FromBase64String(imageBase64);
                
                using var content = new MultipartFormDataContent();
                var imageContent = new ByteArrayContent(imageBytes);
                imageContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/png");
                content.Add(imageContent, "imagen", "document.png");

                // Send to Flask service
                var response = await _httpClient.PostAsync("/segmentar", content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Flask server error during text extraction: {Status} - {Content}", 
                        response.StatusCode, errorContent);
                    
                    return string.Empty;
                }

                var jsonResponse = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<FlaskOCRResponse>(jsonResponse, 
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                // Safe null checking
                if (result?.NombresDetectados == null || result.NombresDetectados.Count == 0)
                {
                    return string.Empty;
                }

                return string.Join("\n", result.NombresDetectados);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error extracting text from image");
                return string.Empty;
            }
        }

        private ProcessedData ParseFlaskResults(List<string>? detectedNames, string documentType)
        {
            var data = new ProcessedData
            {
                Student = new StudentData(),
                Academic = new AcademicData(),
                AdditionalFields = new Dictionary<string, string>()
            };

            if (detectedNames == null || detectedNames.Count == 0)
            {
                return data;
            }

            try
            {
                // Join all detected text
                var fullText = string.Join(" ", detectedNames);
                
                _logger.LogInformation("Parsing text: {Text}", fullText);

                // Simple parsing logic - you can improve this based on your document structure
                ParseAcademicRecord(data, detectedNames, fullText);
                
                // Store detected names
                data.AdditionalFields["detected_names_count"] = detectedNames.Count.ToString();
                data.AdditionalFields["full_text"] = fullText;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing Flask results");
            }

            return data;
        }

        private void ParseAcademicRecord(ProcessedData data, List<string> lines, string fullText)
        {
            foreach (var line in lines)
            {
                var lower = line.ToLower();

                // Look for student name patterns
                if (string.IsNullOrEmpty(data.Student.FullName))
                {
                    // If line looks like a name (contains letters and spaces, no numbers)
                    if (Regex.IsMatch(line, @"^[A-Za-zÁÉÍÓÚáéíóúñÑ\s]+$") && line.Length > 3)
                    {
                        data.Student.FullName = line.Trim();
                    }
                }

                // Look for grade/score patterns
                if (data.Academic?.Score == 0)
                {
                    var scoreMatch = Regex.Match(line, @"(\d{1,3})");
                    if (scoreMatch.Success && int.TryParse(scoreMatch.Groups[1].Value, out var score) && score >= 0 && score <= 100)
                    {
                        data.Academic.Score = score;
                    }
                }

                // Look for subject patterns
                if (string.IsNullOrEmpty(data.Academic?.Subject))
                {
                    var subjectKeywords = new[] { "matemática", "español", "ciencia", "historia", "geografía", "inglés" };
                    if (subjectKeywords.Any(keyword => lower.Contains(keyword)))
                    {
                        data.Academic.Subject = line.Trim();
                    }
                }

                // Look for grade level
                if (string.IsNullOrEmpty(data.Student?.Grade))
                {
                    var gradeMatch = Regex.Match(line, @"(\d+)(?:°|º|o|th|er|do|ro)?");
                    if (gradeMatch.Success)
                    {
                        data.Student.Grade = gradeMatch.Groups[1].Value + "°";
                    }
                }

                // Look for section
                if (string.IsNullOrEmpty(data.Student?.Section))
                {
                    var sectionMatch = Regex.Match(line, @"secci[óo]n\s*([A-Z])", RegexOptions.IgnoreCase);
                    if (sectionMatch.Success)
                    {
                        data.Student.Section = sectionMatch.Groups[1].Value;
                    }
                }
            }

            // If no specific data found, use first line as name
            if (string.IsNullOrEmpty(data.Student?.FullName) && lines.Count > 0)
            {
                data.Student.FullName = lines[0];
            }

            _logger.LogInformation("Parsed data: Student={Student}, Subject={Subject}, Score={Score}",
                data.Student?.FullName, data.Academic?.Subject, data.Academic?.Score);
        }

        private class FlaskOCRResponse
        {
            public List<string>? NombresDetectados { get; set; } = new();
            public List<string>? Filas { get; set; } = new();
            public string? Excel { get; set; } = string.Empty;
        }
    }
}