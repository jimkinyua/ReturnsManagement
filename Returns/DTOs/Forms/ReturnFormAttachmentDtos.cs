using Returns.Helpers.Enums;

namespace Returns.DTOs.Forms
{
    // Request to attach forms to periods
    public class AttachReturnFormsDto
    {
        public List<string> PeriodIds { get; set; } = new();
        public List<string> ReturnFormIds { get; set; } = new();
        public int? FilingDeadlineDays { get; set; } // Days after period end
        public bool ApplyToAllPeriodsInYear { get; set; } = false;
        public string? YearId { get; set; } // Required if ApplyToAllPeriodsInYear is true
    }

    // Preview of what will be created
    public class ReturnFormAttachmentPreviewDto
    {
        public int TotalExpectedReturns { get; set; }
        public List<ExpectedReturnPreviewDto> ExpectedReturns { get; set; } = new();
        public List<string> Warnings { get; set; } = new();
        public string PreviewToken { get; set; } = null!; // Used for confirmation
    }

    // Individual expected return preview
    public class ExpectedReturnPreviewDto
    {
        public string PeriodName { get; set; } = null!;
        public string FormName { get; set; } = null!;
        public string FormCode { get; set; } = null!;
        public DateTime FilingDeadline { get; set; }
        public bool AlreadyExists { get; set; }
        public string? ExistingStatus { get; set; }
    }

    // Confirmation request
    public class ConfirmReturnFormAttachmentDto
    {
        public string PreviewToken { get; set; } = null!;
        public bool ForceOverwrite { get; set; } = false; // If true, overwrites existing
    }

    // Result of attachment operation
    public class ReturnFormAttachmentResultDto
    {
        public bool Success { get; set; }
        public int CreatedCount { get; set; }
        public int UpdatedCount { get; set; }
        public int SkippedCount { get; set; }
        public List<string> Errors { get; set; } = new();
    }

    // Attached form information
    public class AttachedReturnFormDto
    {
        public string Id { get; set; } = null!;
        public string FormId { get; set; } = null!;
        public string FormName { get; set; } = null!;
        public string FormCode { get; set; } = null!;
        public DateTime FilingDeadline { get; set; }
        public ExpectedStatus Status { get; set; }
        public bool IsActive { get; set; }
    }

    // Update filing deadlines
    public class UpdateFilingDeadlinesDto
    {
        public List<string> ExpectedReturnIds { get; set; } = new();
        public DateTime NewFilingDeadline { get; set; }
    }
}