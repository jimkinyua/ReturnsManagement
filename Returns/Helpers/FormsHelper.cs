using Microsoft.EntityFrameworkCore;
using Returns.DTOs.Forms;
using Returns.Helpers.Enums;
using Returns.Models.Data;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Returns.Helpers
{
    public static class FormsHelper
    {
        private static readonly HttpClient _httpClient = new HttpClient();
        private static readonly string _baseUrl = "https://sasra-backend.sasra.go.ke"; // Configurable base URL
        private static IConfiguration _configuration;
        public static async Task ValidateFormTypeUniqueness(CreateFormDTO createFormDTO, ReturnsDbContext context)
        {
            // Check if a form with the same category already exists for this Sacco type
            var existingForm = await context.ReturnForms.FirstOrDefaultAsync(f =>
                f.Category == createFormDTO.Category &&
                f.SaccoTypeId == createFormDTO.SaccoTypeId &&
                f.IsActive);

            if (existingForm != null && createFormDTO.Category != FormCategory.Other)  // Skip for Other
            {
                throw new Exception($"A {createFormDTO.Category} form already exists for this Sacco type. Please deactivate the existing form before creating a new one.");
            }

            // Also check if the form code is unique
            bool codeExists = await context.ReturnForms.AnyAsync(f =>
                f.Code == createFormDTO.Name &&
                f.SaccoTypeId == createFormDTO.SaccoTypeId);

            if (codeExists)
            {
                throw new Exception(
                    $"A form with code '{createFormDTO.Name}' already exists for this Sacco type. Please use a unique code.");
            }
        }
        public static bool IsValidExcelFile(IFormFile file)
        {
            // 1. Check file extension
            var extension = Path.GetExtension(file.FileName).ToLower();
            string sanitizedFileName = Regex.Replace(extension, @"[\\/""\s]+$", ""); // Remove trailing slashes, quotes, and spaces

            if (sanitizedFileName != ".xlsx")
            {
                throw new ValidationException(
                                                        $"'{file.FileName}' is an *.xls* (Excel 97-2003) file. " +
                                                        "The system only accepts *.xlsx* workbooks (Excel 2007 or later). " +
                                                        "Please save the sheet in .xlsx format and upload again.");
            }

            // 2. Check MIME type
            var validMimeTypes = new[]
            {
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
            };

            if (!validMimeTypes.Contains(file.ContentType))
            {
                return false;
            }

            // 3. Check file signature/magic numbers for XLSX
            try
            {
                using (var reader = new BinaryReader(file.OpenReadStream()))
                {
                    var bytes = reader.ReadBytes(4);
                    // XLSX files start with PK.. (ZIP format)
                    if (bytes[0] != 0x50 || bytes[1] != 0x4B || bytes[2] != 0x03 || bytes[3] != 0x04)
                    {
                        return false;
                    }
                }

                // Reset the stream position
                file.OpenReadStream().Position = 0;
                return true;
            }
            catch
            {
                return false;
            }
        }
        public static async Task<string> SaveFileAsync(IFormFile file, string folder, string fileName = "", ILogger logger = null)
        {
            //return "local";
            if (file == null || file.Length <= 0)
            {
                return null;
            }
            try
            {
                // Clean and validate folder name
                folder = string.IsNullOrEmpty(folder) ? "default" : new string(folder.Where(c => !Path.GetInvalidPathChars().Contains(c)).ToArray());
                var ModuleFolder = "Returns";
                folder = Path.Combine(ModuleFolder, folder);

                // Normalize folder for URL with forward slashes
                string urlFolder = folder.Replace('\\', '/');

                string safeFileName;
                if (string.IsNullOrEmpty(fileName))
                {
                    safeFileName = Guid.NewGuid().ToString();
                }
                else
                {
                    var invalidChars = Path.GetInvalidFileNameChars();
                    safeFileName = new string(fileName.Where(ch => !invalidChars.Contains(ch)).ToArray());
                }

                // Add timestamp
                safeFileName = $"{safeFileName}_{DateTime.Now.ToString("yyyyMMddHHmmss")}";

                // Get extension from the original file
                string fileExtension = Path.GetExtension(file.FileName);
                if (string.IsNullOrEmpty(fileExtension))
                {
                    fileExtension = ".bin";
                }

                var fullFileName = safeFileName + fileExtension;

                // Get storage path
                string hostStoragePath = Environment.GetEnvironmentVariable("HOST_STORAGE_PATH") ?? "C:/inetpub/wwwroot/RBSS/Uploads";

                if (string.IsNullOrEmpty(hostStoragePath))
                {
                    throw new Exception("HOST_STORAGE_PATH environment variable is not set.");
                }

                var targetFolderPath = Path.Combine(hostStoragePath, folder);

                // Ensure directory exists with specific error handling
                try
                {
                    Directory.CreateDirectory(targetFolderPath);
                }
                catch (UnauthorizedAccessException uae)
                {
                    logger?.LogError(uae, $"Unauthorized access when creating directory: {targetFolderPath}. Check permissions.");
                    return null;
                }
                catch (PathTooLongException pte)
                {
                    logger?.LogError(pte, $"Path too long for directory: {targetFolderPath}. Consider shortening folder/file names.");
                    return null;
                }
                catch (IOException ioe)
                {
                    logger?.LogError(ioe, $"I/O error creating directory: {targetFolderPath}. Directory may be on a read-only or inaccessible drive.");
                    return null;
                }
                catch (Exception ex)
                {
                    logger?.LogError(ex, $"Unexpected error creating directory: {targetFolderPath}.");
                    return null;
                }

                // Full path to save the file
                var hostFilePath = Path.Combine(targetFolderPath, fullFileName);

                using (var stream = new FileStream(hostFilePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // Log success
                logger?.LogInformation($"File saved at {hostFilePath}");

                // Get file endpoint from environment or use default
                string fileEndpoint = Environment.GetEnvironmentVariable("FILE_API_ENDPOINT") ?? "/api/files";
                if (!fileEndpoint.StartsWith("/"))
                {
                    fileEndpoint = "/" + fileEndpoint;
                }

                // Build URL with forward slashes and escape segments
                string escapedUrlFolder = string.Join("/", urlFolder.Split('/').Select(Uri.EscapeDataString));
                string escapedFileName = Uri.EscapeDataString(fullFileName);

                string fileUrl = $"/gateway{fileEndpoint}/{escapedUrlFolder}/{escapedFileName}";

                return fileUrl;
            }
            catch (Exception ex)
            {
                // Log the exception for file operations (e.g., FileStream creation or CopyToAsync)
                logger?.LogError(ex, $"Error saving file to {folder}: {ex.Message}");
                return null;
            }
        }
        public static async Task<IFormFile?> GetFileFromUrlAsync(string fileUrl)
        {
            try
            {
                if (string.IsNullOrEmpty(fileUrl))
                {
                    Console.WriteLine("File URL is empty or null.");
                    return null;
                }
                // Replace problematic host with local service URL
                fileUrl = fileUrl.Replace("https://sasra-backend.sasra.go.ke", "http://identityservice:8039");
                // Construct full URL if the input is a relative path (starts with /gateway)
                string fullUrl;
                if (fileUrl.StartsWith("/gateway"))
                {
                    // Remove /gateway to bypass it and go directly to the service
                    string relativePath = fileUrl.Substring("/gateway".Length);
                    fullUrl = _baseUrl + relativePath;
                }
                else
                {
                    fullUrl = fileUrl;
                }
                using var response = await _httpClient.GetAsync(fullUrl, HttpCompletionOption.ResponseHeadersRead);
                response.EnsureSuccessStatusCode();
                using var contentStream = await response.Content.ReadAsStreamAsync();
                var memoryStream = new MemoryStream();
                await contentStream.CopyToAsync(memoryStream);
                memoryStream.Position = 0;
                // Extract filename
                string fileName = Path.GetFileName(new Uri(fullUrl).AbsolutePath);
                if (response.Content.Headers.ContentDisposition?.FileNameStar != null)
                {
                    fileName = response.Content.Headers.ContentDisposition.FileNameStar;
                }
                else if (response.Content.Headers.ContentDisposition?.FileName != null)
                {
                    fileName = response.Content.Headers.ContentDisposition.FileName;
                }
                string contentType = response.Content.Headers.ContentType?.MediaType
                    ?? "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                var formFile = new FormFile(memoryStream, 0, memoryStream.Length, Path.GetFileNameWithoutExtension(fileName), fileName)
                {
                    Headers = new HeaderDictionary(),
                    ContentType = contentType
                };
                if (!IsValidExcelFile(formFile))
                {
                    Console.WriteLine($"Invalid Excel file at URL: {fullUrl}");
                    memoryStream.Dispose();
                    return null;
                }
                return formFile;
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"HTTP error retrieving file from URL {fileUrl}: {ex.Message}");
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving file from URL {fileUrl}: {ex.Message}");
                return null;
            }
        }


        public static async Task<string?> SaveReportAsync(byte[] bytes, string folder, string? fileName = null, string extension = ".pdf", CancellationToken ct = default)
        {
            //return "SAVING FILES DISABLED";
            if (bytes is null || bytes.Length == 0)
            {
                return null;
            }

            folder = string.IsNullOrWhiteSpace(folder) ? "SystemReports"
                    : new string(folder.Where(c => !Path.GetInvalidPathChars().Contains(c)).ToArray());

            var invalid = Path.GetInvalidFileNameChars();

            var safeName = string.IsNullOrWhiteSpace(fileName)
                         ? Guid.NewGuid().ToString()
                         : new string(fileName.Where(ch => !invalid.Contains(ch)).ToArray());

            safeName = $"{safeName}_{DateTime.Now:yyyyMMddHHmmss}{extension}";

            var root = Environment.GetEnvironmentVariable("HOST_STORAGE_PATH");
            if (string.IsNullOrWhiteSpace(root))
                throw new InvalidOperationException("HOST_STORAGE_PATH environment variable is not set.");

            root = root.Replace('\\', '/'); // normalize
            var targetDir = Path.Combine(root, folder);
            Directory.CreateDirectory(targetDir);          // idempotent

            var fullPath = Path.Combine(targetDir, safeName);
            await File.WriteAllBytesAsync(fullPath, bytes, ct);
            var endpoint = Environment.GetEnvironmentVariable("FILE_API_ENDPOINT") ?? "/api/files";
            if (!endpoint.StartsWith('/')) endpoint = '/' + endpoint;

            var url = $"/gateway{endpoint}/{folder}/{safeName}";
            return url;
        }
        public static bool DeleteFile(string filePath)
        {
            try
            {
                if (string.IsNullOrEmpty(filePath))
                {
                    return false;
                }

                // Extract the actual file path from the URL if it's a gateway URL
                if (filePath.StartsWith("/gateway"))
                {
                    // Remove gateway prefix and extract the relative path
                    var parts = filePath.Split('/').Skip(3).ToArray(); // Skip empty, "gateway", and "api/files"
                    filePath = string.Join("/", parts);
                }

                // Get the configured storage path from environment variable
                string hostStoragePath = Environment.GetEnvironmentVariable("HOST_STORAGE_PATH");
                if (string.IsNullOrEmpty(hostStoragePath))
                {
                    throw new Exception("HOST_STORAGE_PATH environment variable is not set.");
                }
                hostStoragePath = hostStoragePath.Replace('\\', '/'); // Normalize path separators

                // URL decode the file path to handle encoded spaces and special characters
                filePath = Uri.UnescapeDataString(filePath);

                var fullFilePath = Path.Combine(hostStoragePath, filePath);
                if (File.Exists(fullFilePath))
                {
                    File.Delete(fullFilePath);
                    return true;
                }
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Error deleting file: {ex.Message}");
            }
            return false;
        }

    }
}
