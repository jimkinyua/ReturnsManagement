using Returns.Helpers.Enums;

namespace Returns.DTOs.Forms
{
    public class FormsDueByMonthDTO
    {
        public string FormId { get; set; } = null!;
        public string FormName { get; set; } = null!;
        public string FormCode { get; set; } = null!;
        public string PeriodId { get; set; } = null!;
        public string PeriodName { get; set; } = null!;
        public DateTime PeriodStartDate { get; set; }
        public DateTime PeriodEndDate { get; set; }
        public DateTime FilingDeadline { get; set; }
        public ExpectedStatus Status { get; set; }
        public bool IsLate => DateTime.Now > FilingDeadline && Status != ExpectedStatus.Filed;
        public string? TemplateUrl { get; set; }
        public string SaccoTypeId { get; set; } = null!;
    }
}