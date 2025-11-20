using Google.Cloud.Vision.V1;
using REA.Models.DTOs;
using System.Text.RegularExpressions;

namespace REA.API.Services
{
    public class GoogleOCRService : IOCRService
    {
        private readonly ImageAnnotatorClient _client;
        private readonly ILogger<GoogleOCRService> _logger;

        public GoogleOCRService(IConfiguration configuration, ILogger<GoogleOCRService> logger)
        {
            _logger = logger;
            
            var credentialFilePath = configuration["GoogleCloud:CredentialFilePath"];
            var projectId = configuration["GoogleCloud:ProjectId"];

            try
            {
                if (!string.IsNullOrEmpty(credentialFilePath) && File.Exists(credentialFilePath))
                {
                    Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", credentialFilePath);
                    _logger.LogInformation("Google Cloud credentials loaded from: {Path}", credentialFilePath);
                }
                else
                {
                    _logger.LogWarning("Google Cloud credentials file not found at: {Path}", credentialFilePath);
                }

                _client = ImageAnnotatorClient.Create();
                _logger.LogInformation("Google Cloud Vision API client initialized successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize Google Cloud Vision API client");
                throw;
            }
        }

        public async Task<OCRProcessResponse> ProcessImageAsync(OCRProcessRequest request)
        {
            try
            {
                _logger.LogInformation("Processing OCR request for document type: {DocumentType}", request.DocumentType);

                var extractedText = await ExtractTextFromImageAsync(request.ImageBase64);
                
                if (string.IsNullOrWhiteSpace(extractedText))
                {
                    return new OCRProcessResponse
                    {
                        Success = false,
                        Error = "No se pudo extraer texto de la imagen. Asegúrese de que la imagen contenga texto legible."
                    };
                }

                // Parse the extracted text into structured data
                var processedData = ParseExtractedText(extractedText, request.DocumentType);

                return new OCRProcessResponse
                {
                    Success = true,
                    ExtractedText = extractedText,
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
                var image = Image.FromBytes(imageBytes);
                
                _logger.LogInformation("Sending image to Google Vision API for text detection");
                
                var response = await _client.DetectTextAsync(image);
                
                if (response.Count == 0)
                {
                    _logger.LogWarning("No text detected in image");
                    return string.Empty;
                }

                // The first annotation contains all the text
                var fullText = response[0].Description;
                
                _logger.LogInformation("Successfully extracted {Length} characters of text", fullText.Length);
                
                return fullText;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error extracting text from image");
                throw;
            }
        }

        private ProcessedData ParseExtractedText(string text, string documentType)
        {
            _logger.LogInformation("Parsing extracted text for document type: {DocumentType}", documentType);

            return documentType.ToLower() switch
            {
                "academicrecord" or "graderecord" => ParseAcademicRecord(text),
                "idcard" or "identification" => ParseIdentificationCard(text),
                "attendance" => ParseAttendanceRecord(text),
                _ => ParseGenericDocument(text)
            };
        }

        private ProcessedData ParseAcademicRecord(string text)
        {
            var data = new ProcessedData
            {
                Student = new StudentData(),
                Academic = new AcademicData(),
                AdditionalFields = new Dictionary<string, string>()
            };

            try
            {
                // Extract student name (common patterns: "Estudiante:", "Nombre:", "Alumno:")
                var nameMatch = Regex.Match(text, @"(?:Estudiante|Nombre|Alumno|Student):\s*([A-ZÁÉÍÓÚÑa-záéíóúñ\s]+)", 
                    RegexOptions.IgnoreCase);
                if (nameMatch.Success)
                {
                    data.Student.FullName = nameMatch.Groups[1].Value.Trim();
                }

                // Extract student ID (common patterns: "ID:", "Código:", "Matrícula:")
                var idMatch = Regex.Match(text, @"(?:ID|Código|Matrícula|Code):\s*([A-Z0-9-]+)", 
                    RegexOptions.IgnoreCase);
                if (idMatch.Success)
                {
                    data.Student.StudentId = idMatch.Groups[1].Value.Trim();
                }

                // Extract grade (1st, 2nd, 3rd, etc. or 1°, 2°, 3°, etc.)
                var gradeMatch = Regex.Match(text, @"(?:Grado|Grade):\s*(\d+(?:st|nd|rd|th|°|º)?)", 
                    RegexOptions.IgnoreCase);
                if (gradeMatch.Success)
                {
                    data.Student.Grade = gradeMatch.Groups[1].Value.Trim();
                }

                // Extract section (A, B, C, etc.)
                var sectionMatch = Regex.Match(text, @"(?:Sección|Section):\s*([A-Z])", 
                    RegexOptions.IgnoreCase);
                if (sectionMatch.Success)
                {
                    data.Student.Section = sectionMatch.Groups[1].Value.Trim();
                }

                // Extract subject
                var subjectMatch = Regex.Match(text, @"(?:Materia|Subject|Asignatura):\s*([A-ZÁÉÍÓÚÑa-záéíóúñ\s]+)", 
                    RegexOptions.IgnoreCase);
                if (subjectMatch.Success)
                {
                    data.Academic.Subject = subjectMatch.Groups[1].Value.Trim();
                }

                // Extract score/grade (0-100)
                var scoreMatch = Regex.Match(text, @"(?:Calificación|Grade|Nota|Score):\s*(\d+(?:\.\d+)?)", 
                    RegexOptions.IgnoreCase);
                if (scoreMatch.Success && decimal.TryParse(scoreMatch.Groups[1].Value, out var score))
                {
                    data.Academic.Score = score;
                }

                // Extract period (Primer Parcial, Segundo Parcial, etc.)
                var periodMatch = Regex.Match(text, @"(?:Período|Period|Parcial):\s*([A-ZÁÉÍÓÚÑa-záéíóúñ\s]+)", 
                    RegexOptions.IgnoreCase);
                if (periodMatch.Success)
                {
                    data.Academic.Period = periodMatch.Groups[1].Value.Trim();
                }

                // Extract school year
                var yearMatch = Regex.Match(text, @"(?:Año\s*Escolar|School\s*Year|Año):\s*(\d{4})", 
                    RegexOptions.IgnoreCase);
                if (yearMatch.Success)
                {
                    data.Academic.SchoolYear = yearMatch.Groups[1].Value.Trim();
                }

                // Store all extracted fields for debugging
                data.AdditionalFields["raw_text_length"] = text.Length.ToString();
                data.AdditionalFields["extraction_confidence"] = CalculateConfidence(data).ToString("P0");

                _logger.LogInformation("Academic record parsed: Student={Student}, Subject={Subject}, Score={Score}", 
                    data.Student?.FullName, data.Academic?.Subject, data.Academic?.Score);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing academic record");
            }

            return data;
        }

        private ProcessedData ParseIdentificationCard(string text)
        {
            var data = new ProcessedData
            {
                Student = new StudentData(),
                AdditionalFields = new Dictionary<string, string>()
            };

            try
            {
                // Extract full name
                var nameMatch = Regex.Match(text, @"(?:Nombre|Name):\s*([A-ZÁÉÍÓÚÑa-záéíóúñ\s]+)", 
                    RegexOptions.IgnoreCase);
                if (nameMatch.Success)
                {
                    data.Student.FullName = nameMatch.Groups[1].Value.Trim();
                }

                // Extract ID number
                var idMatch = Regex.Match(text, @"(?:ID|Cédula|DNI|Matrícula):\s*([A-Z0-9-]+)", 
                    RegexOptions.IgnoreCase);
                if (idMatch.Success)
                {
                    data.Student.StudentId = idMatch.Groups[1].Value.Trim();
                }

                _logger.LogInformation("ID card parsed: Student={Student}, ID={Id}", 
                    data.Student?.FullName, data.Student?.StudentId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing identification card");
            }

            return data;
        }

        private ProcessedData ParseAttendanceRecord(string text)
        {
            var data = new ProcessedData
            {
                Student = new StudentData(),
                AdditionalFields = new Dictionary<string, string>()
            };

            try
            {
                // Extract student name
                var nameMatch = Regex.Match(text, @"(?:Estudiante|Nombre|Student):\s*([A-ZÁÉÍÓÚÑa-záéíóúñ\s]+)", 
                    RegexOptions.IgnoreCase);
                if (nameMatch.Success)
                {
                    data.Student.FullName = nameMatch.Groups[1].Value.Trim();
                }

                // Extract attendance status
                var statusMatch = Regex.Match(text, @"(?:Estado|Status|Asistencia):\s*([A-ZÁÉÍÓÚÑa-záéíóúñ\s]+)", 
                    RegexOptions.IgnoreCase);
                if (statusMatch.Success)
                {
                    data.AdditionalFields["attendance_status"] = statusMatch.Groups[1].Value.Trim();
                }

                _logger.LogInformation("Attendance record parsed: Student={Student}", 
                    data.Student?.FullName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing attendance record");
            }

            return data;
        }

        private ProcessedData ParseGenericDocument(string text)
        {
            var data = new ProcessedData
            {
                AdditionalFields = new Dictionary<string, string>
                {
                    ["text_preview"] = text.Length > 200 ? text.Substring(0, 200) + "..." : text,
                    ["total_length"] = text.Length.ToString()
                }
            };

            _logger.LogInformation("Generic document parsed with {Length} characters", text.Length);

            return data;
        }

        private double CalculateConfidence(ProcessedData data)
        {
            var fieldsFound = 0;
            var totalFields = 8; // Total expected fields

            if (!string.IsNullOrEmpty(data.Student?.FullName)) fieldsFound++;
            if (!string.IsNullOrEmpty(data.Student?.StudentId)) fieldsFound++;
            if (!string.IsNullOrEmpty(data.Student?.Grade)) fieldsFound++;
            if (!string.IsNullOrEmpty(data.Student?.Section)) fieldsFound++;
            if (!string.IsNullOrEmpty(data.Academic?.Subject)) fieldsFound++;
            if (data.Academic?.Score.HasValue == true) fieldsFound++;
            if (!string.IsNullOrEmpty(data.Academic?.Period)) fieldsFound++;
            if (!string.IsNullOrEmpty(data.Academic?.SchoolYear)) fieldsFound++;

            return (double)fieldsFound / totalFields;
        }
    }
}