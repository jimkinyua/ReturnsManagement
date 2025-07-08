using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Returns.DTOs.Returns_Submission.DT
{
    public class ReturnFormUploadDTO
    {
        public IFormFile?  formFile {  get; set; }
        public string ExpectedReturnId { get; set; } = null!;
        public string? FormId { get; set; }
    }
}
