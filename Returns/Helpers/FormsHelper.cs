using Microsoft.EntityFrameworkCore;
using Returns.Models.Data;

namespace Returns.Helpers
{
    public static class FormsHelper
    {
        public static async Task ValidateFormTypeUniqueness(CreateFormDTO createFormDTO)
        {
            var context = new ReturnsDbContext();
            // Get existing forms for this Sacco type and frequency that are not deleted
            var existingForms = await context.ReturnForms.ToListAsync();

            // Check for Capital Adequacy Form
            if (createFormDTO.IsCapitalAdequencyForm)
            {
                bool exists = existingForms.Any(f =>
                    f.IsCapitalAdequencyForm &&
                    f.PeriodId == createFormDTO.PeriodId &&
                    f.SaccoTypeId == createFormDTO.SaccoTypeId);

                if (exists)
                {
                    throw new Exception(
                        "A Capital Adequacy form already exists for this period and Sacco type.");
                }
            }

            // Check for Liquidity Statement
            if (createFormDTO.IsLiquidityStatement)
            {
                bool exists = existingForms.Any(f =>
                    f.IsLiquidityStatement &&
                    f.PeriodId == createFormDTO.PeriodId &&
                    f.SaccoTypeId == createFormDTO.SaccoTypeId);

                if (exists)
                {
                    throw new Exception(
                        "A Liquidity Statement form already exists for this frequency and Sacco type.");
                }
            }

            // Similar checks for other form types
            if (createFormDTO.IsRiskClassification &&
                existingForms.Any(f => f.IsRiskClassification &&
                f.PeriodId == createFormDTO.PeriodId &&
                f.SaccoTypeId == createFormDTO.SaccoTypeId))
            {
                throw new Exception(
                    "A Risk Classification form already exists for this frequency and Sacco type.");
            }

            if (createFormDTO.IsInvestmentReturn &&
                existingForms.Any(f => f.IsInvestmentReturn &&
                f.PeriodId == createFormDTO.PeriodId &&
                f.SaccoTypeId == createFormDTO.SaccoTypeId))
            {
                throw new Exception(
                    "An Investment Return form already exists for this frequency and Sacco type.");
            }

            if (createFormDTO.IsFinancialPosition &&
                existingForms.Any(f => f.IsFinancialPosition &&
                f.PeriodId == createFormDTO.PeriodId &&
                f.SaccoTypeId == createFormDTO.SaccoTypeId))
            {
                throw new Exception(
                    "A Financial Position form already exists for this frequency and Sacco type.");
            }

            if (createFormDTO.IsStatementOfComprehensiveIncome &&
                existingForms.Any(f => f.IsStatementOfComprehensiveIncome &&
                f.PeriodId == createFormDTO.PeriodId &&
                f.SaccoTypeId == createFormDTO.SaccoTypeId))
            {
                throw new Exception(
                    "A Statement of Comprehensive Income form already exists for this frequency and Sacco type.");
            }

            if (createFormDTO.IsDepositReturnForm &&
                existingForms.Any(f => f.IsDepositReturnForm &&
                f.PeriodId == createFormDTO.PeriodId &&
                f.SaccoTypeId == createFormDTO.SaccoTypeId))
            {
                throw new Exception(
                    "A Deposit Return form already exists for this frequency and Sacco type.");
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
            if (file == null || file.Length <= 0)
            {
                return null;
            }

            try
            {
                folder = string.IsNullOrEmpty(folder) ? "default" :
                    new string(folder.Where(c => !Path.GetInvalidPathChars().Contains(c)).ToArray());

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
                    // Default to .bin if no extension
                    fileExtension = ".bin";
                }

                var fullFileName = safeFileName + fileExtension;

                // Get the configured storage path from environment variable with proper handling for different path formats
                string hostStoragePath = Environment.GetEnvironmentVariable("HOST_STORAGE_PATH") ?? "C:/inetpub/wwwroot/RBSS/Uploads";
                //return "SAVING FILES DISABLED";
                if (string.IsNullOrEmpty(hostStoragePath))
                {
                    throw new Exception("HOST_STORAGE_PATH environment variable is not set.");
                }

                hostStoragePath = hostStoragePath.Replace('\\', '/'); // Normalize path separators

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

                // Generate URL for the file that works with API Gateway
                string fileUrl = $"/gateway{fileEndpoint}/{folder}/{fullFileName}";

                return fileUrl;
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Error saving file: {ex.Message}");
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



        // DeleteFile
        public static async Task<bool> DeleteFile(string filePath)
        {
            try
            {
                if (string.IsNullOrEmpty(filePath))
                {
                    return false;
                }
                // Get the configured storage path from environment variable
                string hostStoragePath = Environment.GetEnvironmentVariable("HOST_STORAGE_PATH");
                if (string.IsNullOrEmpty(hostStoragePath))
                {
                    throw new Exception("HOST_STORAGE_PATH environment variable is not set.");
                }
                hostStoragePath = hostStoragePath.Replace('\\', '/'); // Normalize path separators
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
