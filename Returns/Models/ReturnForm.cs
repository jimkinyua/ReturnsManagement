using Returns.Helpers.Enums;
using Returns.Models.Common;

namespace Returns.Models
{
    public class ReturnForm : CommonFields
    {
        public string Code { get; set; } = null!;  // unique within SaccoType
        public string FormName { get; set; } = null!;
        public string TemplateUrl { get; set; } = null!;
        public string SaccoTypeId { get; set; } = null!;  // FK → SaccoTypes
        public FormCategory Category { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
