using System.Net.Http.Headers;
using System.Text.Json;
using REA.Models.DTOs;

namespace REA.API.Services
{
    public class FlaskOCRService : IOCRService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<FlaskOCRService> _logger;
        private readonly string _flaskServerUrl;

        public FlaskOCRService(
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration,
            ILogger<FlaskOCRService> logger)
        {
            _httpClient = httpClientFactory.CreateClient("FlaskOCR");
            _logger = logger;
            _flaskServerUrl = configuration["OCR:FlaskServerUrl"] ?? "http://localhost:5000";
            
            _logger.LogInformation("Flask OCR Service initialized with URL: {Url}", _flaskServerUrl);
        }

        public async Task<OCRProcessResponse> ProcessImageAsync(OCRProcessRequest request)
        {
            try
            {
                _logger.LogInformation("Processing OCR request for document type: {DocumentType}", 
                    request.DocumentType);

                // Convert base64 to bytes
                var imageBytes = Convert.FromBase64String(request.ImageBase64);
                
                _logger.LogInformation("Image size: {Size} bytes", imageBytes.Length);

                // Send to Flask service
                var result = await SendToFlaskService(imageBytes);

                if (result == null)
                {
                    return new OCRProcessResponse
                    {
                        Success = false,
                        Error = "No se pudo procesar la imagen con el servidor OCR"
                    };
                }

                // Parse results
                var processedData = ParseFlaskResults(result.NombresDetectados, request.DocumentType);

                return new OCRProcessResponse
                {
                    Success = true,
                    ExtractedText = string.Join("\n", result.NombresDetectados),
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
                var imageBytes = Convert.FromBase64String(imageBase64);
                var result = await SendToFlaskService(imageBytes);
                
                return result != null 
                    ? string.Join("\n", result.NombresDetectados) 
                    : string.Empty;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error extracting text from image");
                throw;
            }
        }

        private async Task<FlaskOCRResult?> SendToFlaskService(byte[] imageBytes)
        {
            try
            {
                using var content = new MultipartFormDataContent();
                var imageContent = new ByteArrayContent(imageBytes);
                imageContent.Headers.ContentType = MediaTypeHeaderValue.Parse("image/png");
                content.Add(imageContent, "imagen", "foto.png");

                _logger.LogInformation("Sending image to Flask server: {Url}/segmentar", _flaskServerUrl);

                var response = await _httpClient.PostAsync($"{_flaskServerUrl}/segmentar", content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Flask server error: {Status} - {Content}", 
                        response.StatusCode, errorContent);
                    return null;
                }

                var jsonResponse = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("Flask response received: {Length} chars", jsonResponse.Length);

                var result = JsonSerializer.Deserialize<FlaskOCRResult>(jsonResponse, 
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (result?.NombresDetectados != null)
                {
                    _logger.LogInformation("Detected {Count} names from OCR", 
                        result.NombresDetectados.Count);
                }

                return result;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP error communicating with Flask server");
                throw new Exception($"No se pudo conectar al servidor OCR: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending request to Flask service");
                throw;
            }
        }

        private ProcessedData ParseFlaskResults(List<string> detectedNames, string documentType)
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

                // Parse based on document type
                switch (documentType.ToLower())
                {
                    case "academicrecord":
                    case "graderecord":
                        ParseAcademicRecord(data, detectedNames, fullText);
                        break;
                    case "attendance":
                        ParseAttendanceRecord(data, detectedNames, fullText);
                        break;
                    default:
                        ParseGenericDocument(data, detectedNames);
                        break;
                }

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

                // Try to extract student name (usually first line or after "estudiante:")
                if (string.IsNullOrEmpty(data.Student.FullName) && 
                    (lower.Contains("estudiante") || lower.Contains("nombre") || lower.Contains("alumno")))
                {
                    var parts = line.Split(':', 2);
                    if (parts.Length > 1)
                    {
                        data.Student.FullName = parts[1].Trim();
                    }
                }
                // If no label found and line has letters, assume first meaningful line is name
                else if (string.IsNullOrEmpty(data.Student.FullName) && 
                         System.Text.RegularExpressions.Regex.IsMatch(line, @"[A-Za-zÁÉÍÓÚáéíóúñÑ\s]{5,}"))
                {
                    data.Student.FullName = line.Trim();
                }

                // Extract grade
                if (lower.Contains("grado") || lower.Contains("grade"))
                {
                    var match = System.Text.RegularExpressions.Regex.Match(line, @"(\d+)[°º]?");
                    if (match.Success)
                    {
                        data.Student.Grade = match.Groups[1].Value + "th";
                    }
                }

                // Extract section
                if (lower.Contains("sección") || lower.Contains("seccion") || lower.Contains("section"))
                {
                    var match = System.Text.RegularExpressions.Regex.Match(line, @"\b([A-Z])\b");
                    if (match.Success)
                    {
                        data.Student.Section = match.Groups[1].Value;
                    }
                }

                // Extract subject
                if (lower.Contains("materia") || lower.Contains("subject") || lower.Contains("asignatura"))
                {
                    var parts = line.Split(':', 2);
                    if (parts.Length > 1)
                    {
                        data.Academic.Subject = parts[1].Trim();
                    }
                }

                // Extract score/grade
                if (lower.Contains("calificación") || lower.Contains("nota") || lower.Contains("score"))
                {
                    var match = System.Text.RegularExpressions.Regex.Match(line, @"(\d+(?:\.\d+)?)");
                    if (match.Success && decimal.TryParse(match.Groups[1].Value, out var score))
                    {
                        data.Academic.Score = score;
                    }
                }

                // Extract period
                if (lower.Contains("período") || lower.Contains("periodo") || lower.Contains("parcial"))
                {
                    var parts = line.Split(':', 2);
                    if (parts.Length > 1)
                    {
                        data.Academic.Period = parts[1].Trim();
                    }
                }

                // Extract school year
                if (lower.Contains("año") && lower.Contains("escolar"))
                {
                    var match = System.Text.RegularExpressions.Regex.Match(line, @"(\d{4})");
                    if (match.Success)
                    {
                        data.Academic.SchoolYear = match.Groups[1].Value;
                    }
                }
            }

            _logger.LogInformation("Parsed academic record: Student={Student}, Subject={Subject}, Score={Score}",
                data.Student?.FullName, data.Academic?.Subject, data.Academic?.Score);
        }

        private void ParseAttendanceRecord(ProcessedData data, List<string> lines, string fullText)
        {
            // First line is usually the name
            if (lines.Count > 0)
            {
                data.Student.FullName = lines[0].Trim();
            }

            // Look for attendance status
            foreach (var line in lines)
            {
                var lower = line.ToLower();
                if (lower.Contains("presente") || lower.Contains("present"))
                {
                    data.AdditionalFields["attendance_status"] = "Presente";
                }
                else if (lower.Contains("ausente") || lower.Contains("absent"))
                {
                    data.AdditionalFields["attendance_status"] = "Ausente";
                }
            }

            _logger.LogInformation("Parsed attendance: Student={Student}", data.Student?.FullName);
        }

        private void ParseGenericDocument(ProcessedData data, List<string> lines)
        {
            // Store all lines as detected names
            for (int i = 0; i < lines.Count; i++)
            {
                data.AdditionalFields[$"line_{i + 1}"] = lines[i];
            }

            _logger.LogInformation("Parsed generic document with {Count} lines", lines.Count);
        }

        private class FlaskOCRResult
        {
            public List<string> Filas { get; set; } = new();
            public string Excel { get; set; } = string.Empty;
            public List<string> NombresDetectados { get; set; } = new();
        }
    }
}