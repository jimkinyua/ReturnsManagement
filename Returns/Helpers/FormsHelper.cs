using Microsoft.EntityFrameworkCore;
using Returns.DTOs.Forms;
using Returns.Models.Data;

namespace Returns.Helpers
{
    public static class FormsHelper
    {
        public static async Task ValidateFormTypeUniqueness(CreateFormDTO createFormDTO, ReturnsDbContext context)
        {
            // Check if a form with the same category already exists for this Sacco type
            bool exists = await context.ReturnForms.AnyAsync(f =>
                f.Category == createFormDTO.Category &&
                f.SaccoTypeId == createFormDTO.SaccoTypeId &&
                f.IsActive);

            if (exists)
            {
                var categoryName = createFormDTO.Category.ToString().Replace("_", " ");
                throw new Exception(
                    $"A {categoryName} form already exists for this Sacco type. Please deactivate the existing form before creating a new one.");
            }

            // Also check if the form code is unique
            bool codeExists = await context.ReturnForms.AnyAsync(f =>
                f.Code == createFormDTO.DisplayName &&
                f.SaccoTypeId == createFormDTO.SaccoTypeId);

            if (codeExists)
            {
                throw new Exception(
                    $"A form with code '{createFormDTO.DisplayName}' already exists for this Sacco type. Please use a unique code.");
            }
        }
        public static bool IsValidExcelFile(IFormFile file)
        {
            // 1. Check file extension
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (extension != ".xlsx")
            {
                return false;
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
        public static async Task<string> SaveFileAsync(IFormFile file, string folder, string fileName = "")
        {
            return "Well..";
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
                // Ensure the folder name is safe
                if (folder.IndexOfAny(Path.GetInvalidPathChars()) >= 0)
                {
                    throw new ArgumentException("Folder name contains invalid characters.");
                }

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

                // Ensure directory exists
                Directory.CreateDirectory(targetFolderPath);

                // Full path to save the file
                var hostFilePath = Path.Combine(targetFolderPath, fullFileName);

                using (var stream = new FileStream(hostFilePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // Get file endpoint from environment or use default
                string fileEndpoint = Environment.GetEnvironmentVariable("FILE_API_ENDPOINT") ?? "/api/files";
                if (!fileEndpoint.StartsWith("/"))
                {
                    fileEndpoint = "/" + fileEndpoint;
                }

                // URL encode the folder and filename to handle spaces and special characters
                string encodedFolder = Uri.EscapeDataString(folder);
                string encodedFileName = Uri.EscapeDataString(fullFileName);

                // Generate URL for the file that works with API Gateway
                string fileUrl = $"/gateway{fileEndpoint}/{encodedFolder}/{encodedFileName}";

                return fileUrl;
            }
            catch (Exception ex)
            {
                // Log the exception (consider using proper logging instead of Console.WriteLine)
                Console.WriteLine($"Error saving file: {ex.Message}");
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

                // Extract the actual file path from the URL
                string relativePath = fileUrl;
                if (fileUrl.StartsWith("/gateway"))
                {
                    // Remove gateway prefix and extract the relative path
                    var parts = fileUrl.Split('/').Skip(3).ToArray(); // Skip empty, "gateway", and "api/files"
                    relativePath = string.Join("/", parts);
                }

                // Get the configured storage path from environment variable
                string hostStoragePath = Environment.GetEnvironmentVariable("HOST_STORAGE_PATH") ?? "C:/inetpub/wwwroot/RBSS/Uploads";
                if (string.IsNullOrEmpty(hostStoragePath))
                {
                    throw new InvalidOperationException("HOST_STORAGE_PATH environment variable is not set.");
                }
                hostStoragePath = hostStoragePath.Replace('\\', '/'); // Normalize path separators

                // URL decode the file path to handle encoded spaces and special characters
                relativePath = Uri.UnescapeDataString(relativePath);

                // Construct the full file path
                var fullFilePath = Path.Combine(hostStoragePath, relativePath);
                if (!File.Exists(fullFilePath))
                {
                    Console.WriteLine($"File not found at path: {fullFilePath}");
                    return null;
                }

                // Read the file into a MemoryStream
                var memoryStream = new MemoryStream();
                using (var fileStream = new FileStream(fullFilePath, FileMode.Open, FileAccess.Read))
                {
                    await fileStream.CopyToAsync(memoryStream);
                }
                memoryStream.Position = 0; // Reset stream position

                // Create FormFile
                var fileName = Path.GetFileName(fullFilePath);
                var formFile = new FormFile(memoryStream, 0, memoryStream.Length, fileName, fileName)
                {
                    Headers = new HeaderDictionary(),
                    ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" // Assume XLSX
                };

                // Validate Excel file
                if (!IsValidExcelFile(formFile))
                {
                    Console.WriteLine($"Invalid Excel file at path: {fullFilePath}");
                    memoryStream.Dispose();
                    return null;
                }

                return formFile;
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
