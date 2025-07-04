using Microsoft.AspNetCore.Http;
using Returns.Helpers.Enums;

namespace Returns.DTOs.Forms
{
    public class UpdateFormDTO
    {
        public string Name { get; set; } = null!;
        public string DisplayName { get; set; } = null!;  // This is the Code
        public string SaccoTypeId { get; set; } = null!;
        public FormCategory Category { get; set; }
        public IFormFile? Template { get; set; }  // Optional - only if updating template
        public bool UpdateTemplate { get; set; } = false;  // Flag to indicate if template should be updated
    }
}