using Returns.Helpers.Enums;

namespace Returns.DTOs.Forms
{
    public class FormDTO
    {
        public string Id { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string DisplayName { get; set; } = null!;  // This is the Code
        public string SaccoTypeId { get; set; } = null!;
        public string TemplateUrl { get; set; } = null!;
        public FormCategory Category { get; set; }
        public string Frequency { get; set; } = "ALL";
        public Boolean IsActive { get; set; } = true;
    }
}
