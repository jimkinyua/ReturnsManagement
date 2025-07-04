namespace Returns.DTOs.Forms
{
    public class FormsDueSummaryDTO
    {
        public int TotalExpectedReturns { get; set; }
        public int DueCount { get; set; }
        public int LateCount { get; set; }
        public int FiledCount { get; set; }
        public int WaivedCount { get; set; }
        public decimal ComplianceRate => TotalExpectedReturns > 0 ? 
            Math.Round((decimal)FiledCount / TotalExpectedReturns * 100, 2) : 0;
        public List<FormsDueSummaryByFormDTO> ByForm { get; set; } = new();
    }

    public class FormsDueSummaryByFormDTO
    {
        public string FormName { get; set; } = null!;
        public string FormCode { get; set; } = null!;
        public int TotalExpected { get; set; }
        public int DueCount { get; set; }
        public int LateCount { get; set; }
        public int FiledCount { get; set; }
        public int WaivedCount { get; set; }
    }
}