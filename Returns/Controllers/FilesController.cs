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
    [Route("api/returns")]
    [ApiController]
    public class FilesController : ControllerBase
    {
        private readonly ILogger<FilesController> _logger;
        private readonly string _hostStoragePath;
        private readonly FileExtensionContentTypeProvider _contentTypeProvider;

        public FilesController(ILogger<FilesController> logger)
        {
            _logger = logger;
            _hostStoragePath = "C:/inetpub/wwwroot/RBSS/Uploads"; //Environment.GetEnvironmentVariable("HOST_STORAGE_PATH") ?? "C:/inetpub/wwwroot/RBSS/Uploads";
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

        [HttpGet("{folder}/{filename}")]
        public async Task<IActionResult> GetFile(string folder, string filename, [FromQuery] bool download = false)
        {
            try
            {
                // Validate input to prevent path traversal attacks
                if (string.IsNullOrEmpty(folder) || string.IsNullOrEmpty(filename) ||
                    folder.Contains("..") || filename.Contains("..") ||
                    Path.GetInvalidPathChars().Any(c => folder.Contains(c) || filename.Contains(c)))
                {
                    return BadRequest("Invalid folder or filename");
                }

                var filePath = Path.Combine(_hostStoragePath, folder, filename);

                // Check if file exists
                if (!System.IO.File.Exists(filePath))
                {
                    _logger.LogWarning($"File not found: {filePath}");
                    return NotFound($"The file {filename} was not found.");
                }

                // Determine content type
                if (!_contentTypeProvider.TryGetContentType(filePath, out var contentType))
                {
                    contentType = "application/octet-stream";
                }

                // Read the file
                var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);

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
                    enableRangeProcessing: true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving file {folder}/{filename}");
                return StatusCode(500, "An error occurred while retrieving the file.");
            }
        }

        [HttpGet("list/{folder?}")]
        public IActionResult ListFiles(string folder = "")
        {
            try
            {
                // Validate folder name
                if (folder != null && (folder.Contains("..") || Path.GetInvalidPathChars().Any(c => folder.Contains(c))))
                {
                    return BadRequest("Invalid folder name");
                }

                string targetPath = string.IsNullOrEmpty(folder)
                    ? _hostStoragePath
                    : Path.Combine(_hostStoragePath, folder);

                if (!Directory.Exists(targetPath))
                {
                    return NotFound($"Folder '{folder}' not found");
                }

                var files = Directory.GetFiles(targetPath)
                    .Select(filePath => new FileInfo(filePath))
                    .Select(fileInfo => new
                    {
                        name = fileInfo.Name,
                        size = fileInfo.Length,
                        lastModified = fileInfo.LastWriteTime,
                        type = GetFileType(fileInfo.Extension),
                        url = $"/gateway/api/files/{folder}/{fileInfo.Name}",
                        downloadUrl = $"/gateway/api/files/{folder}/{fileInfo.Name}?download=true"
                    })
                    .ToList();

                return Ok(new { files });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error listing files in folder '{folder}'");
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