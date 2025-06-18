using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Returns.Controllers
{
    [Route("api/files")]
    [ApiController]
    public class FilesController : ControllerBase
    {
        private readonly ILogger<FilesController> _logger;
        private readonly string _hostStoragePath;
        private readonly FileExtensionContentTypeProvider _contentTypeProvider;

        public FilesController(ILogger<FilesController> logger)
        {
            _logger = logger;
            _hostStoragePath = Environment.GetEnvironmentVariable("HOST_STORAGE_PATH") ?? "C:/inetpub/wwwroot/RBSS/Uploads";
            _contentTypeProvider = new FileExtensionContentTypeProvider();
            AddContentTypeMappings(_contentTypeProvider.Mappings);
        }

        private void AddContentTypeMappings(IDictionary<string, string> mappings)
        {
            // Microsoft Office formats
            mappings[".xlsx"] = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            mappings[".xls"] = "application/vnd.ms-excel";
            mappings[".docx"] = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
            mappings[".doc"] = "application/msword";
            mappings[".pptx"] = "application/vnd.openxmlformats-officedocument.presentationml.presentation";
            mappings[".ppt"] = "application/vnd.ms-powerpoint";

            // PDF documents
            mappings[".pdf"] = "application/pdf";

            // Images
            mappings[".jpg"] = "image/jpeg";
            mappings[".jpeg"] = "image/jpeg";
            mappings[".png"] = "image/png";
            mappings[".gif"] = "image/gif";
            mappings[".bmp"] = "image/bmp";
            mappings[".tiff"] = "image/tiff";

            // Other common formats
            mappings[".txt"] = "text/plain";
            mappings[".csv"] = "text/csv";
            mappings[".xml"] = "application/xml";
            mappings[".zip"] = "application/zip";
            mappings[".rar"] = "application/x-rar-compressed";
        }

        [HttpGet("{*filepath}")]
        public async Task<IActionResult> GetFile(string filepath, [FromQuery] bool download = false)
        {
            try
            {
                // Validate input to prevent path traversal attacks
                if (string.IsNullOrEmpty(filepath) ||
                    filepath.Contains("..") ||
                    Path.GetInvalidPathChars().Any(c => filepath.Contains(c)))
                {
                    return BadRequest("Invalid file path");
                }

                // Split the path to get folder structure and filename
                var pathParts = filepath.Split('/', StringSplitOptions.RemoveEmptyEntries);
                if (pathParts.Length == 0)
                {
                    return BadRequest("Invalid file path");
                }

                var filename = pathParts.Last();
                var folderPath = string.Join(Path.DirectorySeparatorChar, pathParts.Take(pathParts.Length - 1));
                var fullPath = string.IsNullOrEmpty(folderPath)? Path.Combine(_hostStoragePath, filename):Path.Combine(_hostStoragePath, folderPath, filename);

                // Security check: ensure the resolved path is within the storage directory
                var resolvedPath = Path.GetFullPath(fullPath);
                var storagePath = Path.GetFullPath(_hostStoragePath);
                if (!resolvedPath.StartsWith(storagePath, StringComparison.OrdinalIgnoreCase))
                {
                    return BadRequest("Access to the specified file is denied.");
                }

                if (!System.IO.File.Exists(resolvedPath))
                {
                    _logger.LogWarning($"File not found: {resolvedPath}");
                    return NotFound($"The file {filename} was not found.");
                }

                if (!_contentTypeProvider.TryGetContentType(resolvedPath, out var contentType))
                {
                    contentType = "application/octet-stream";
                }

                var fileBytes = await System.IO.File.ReadAllBytesAsync(resolvedPath);

                // Determine if the file should be shown inline or as an attachment
                string disposition = download ? "attachment" : "inline";

                // Some browsers need special handling for certain file types
                string extension = Path.GetExtension(filename).ToLowerInvariant();
                bool shouldForceDownload = extension == ".xlsx" || extension == ".xls" ||
                                          extension == ".docx" || extension == ".doc" ||
                                          extension == ".pptx" || extension == ".ppt" ||
                                          extension == ".zip" || extension == ".rar";

                if (shouldForceDownload)
                {
                    disposition = "attachment";
                }

                // Set content disposition header
                Response.Headers.Append("Content-Disposition", $"{disposition}; filename=\"{filename}\"");
               
                return File(
                  fileBytes,
                  contentType,
                  enableRangeProcessing: true
                );

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving file {filepath}");
                return StatusCode(500, "An error occurred while retrieving the file.");
            }
        }

        [HttpGet("list/{*folderPath}")]
        public IActionResult ListFiles(string folderPath = "")
        {
            try
            {
                // Handle URL decoding and path validation
                if (!string.IsNullOrEmpty(folderPath))
                {
                    folderPath = Uri.UnescapeDataString(folderPath);
                    if (folderPath.Contains("..") || Path.GetInvalidPathChars().Any(c => folderPath.Contains(c)))
                    {
                        return BadRequest("Invalid folder path");
                    }
                }

                string targetPath = string.IsNullOrEmpty(folderPath)
                    ? _hostStoragePath
                    : Path.Combine(_hostStoragePath, folderPath);

                if (!Directory.Exists(targetPath))
                {
                    return NotFound($"Folder '{folderPath}' not found");
                }

                var files = Directory.GetFiles(targetPath)
                    .Select(filePath => new FileInfo(filePath))
                    .Select(fileInfo => new
                    {
                        name = fileInfo.Name,
                        size = fileInfo.Length,
                        lastModified = fileInfo.LastWriteTime,
                        type = GetFileType(fileInfo.Extension),
                        relativePath = string.IsNullOrEmpty(folderPath)
                            ? fileInfo.Name
                            : $"{folderPath}/{fileInfo.Name}",
                        url = $"/gateway/api/files/{(string.IsNullOrEmpty(folderPath) ? fileInfo.Name : $"{folderPath}/{fileInfo.Name}")}",
                        downloadUrl = $"/gateway/api/files/{(string.IsNullOrEmpty(folderPath) ? fileInfo.Name : $"{folderPath}/{fileInfo.Name}")}?download=true"
                    })
                    .ToList();

                var folders = Directory.GetDirectories(targetPath)
                    .Select(dirPath => new DirectoryInfo(dirPath))
                    .Select(dirInfo => new
                    {
                        name = dirInfo.Name,
                        type = "folder",
                        relativePath = string.IsNullOrEmpty(folderPath)
                            ? dirInfo.Name
                            : $"{folderPath}/{dirInfo.Name}"
                    })
                    .ToList();

                return Ok(new { files, folders });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error listing files in folder '{folderPath}'");
                return StatusCode(500, "An error occurred while listing files");
            }
        }

        private string GetFileType(string extension)
        {
            if (string.IsNullOrEmpty(extension))
                return "unknown";

            extension = extension.ToLowerInvariant();

            if (new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".tiff" }.Contains(extension))
                return "image";

            if (new[] { ".pdf" }.Contains(extension))
                return "pdf";

            if (new[] { ".doc", ".docx" }.Contains(extension))
                return "word";

            if (new[] { ".xls", ".xlsx" }.Contains(extension))
                return "excel";

            if (new[] { ".ppt", ".pptx" }.Contains(extension))
                return "powerpoint";

            if (new[] { ".txt", ".csv" }.Contains(extension))
                return "text";

            if (new[] { ".zip", ".rar" }.Contains(extension))
                return "archive";

            return "other";
        }
    }
}