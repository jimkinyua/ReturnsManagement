using Returns.Helpers.Enums;

namespace Returns.DTOs.Forms
{
    public class FormCategorySummaryDTO
    {
        public FormCategory Category { get; set; }
        public string CategoryName => Category.ToString();
        public int FormCount { get; set; }
        public int ActiveFormCount { get; set; }
        public int InactiveFormCount { get; set; }
    }
}