using Returns.Models.Common;

namespace Returns.Models
{
    public class ReportReadniess:CommonFields
    {
        public string PeriodId { get; set; } = string.Empty;
        public string SaccoId { get; set; } = string.Empty;
        public Boolean IsApproved { get; set; }
    }
}
