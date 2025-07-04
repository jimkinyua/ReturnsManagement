using Returns.Helpers.Enums;

namespace Returns.DTOs.Returns
{
    public class ExpectedReturnDto
    {
        public string Id { get; set; } = "";
        public string PeriodId { get; set; } = "";
        public string PeriodName { get; set; } = "";
        public DateTime PeriodStartDate { get; set; }
        public DateTime PeriodEndDate { get; set; }
        public string FormId { get; set; } = "";
        public string FormCode { get; set; } = "";
        public string FormName { get; set; } = "";
        public DateTime DueDate { get; set; }
        public ExpectedStatus Status { get; set; }
        public int DaysUntilDue { get; set; }
        public bool CanFile { get; set; }
        
        // Waiver information if applicable
        public bool IsWaived { get; set; }
        public string? WaivedReason { get; set; }
        public DateTime? WaivedDate { get; set; }
        
        // Filing information if filed
        public string? ReturnId { get; set; }
        public DateTime? FiledDate { get; set; }
        public bool? IsLate { get; set; }
    }
}